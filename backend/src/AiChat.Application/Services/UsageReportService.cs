using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.UserAggregate;
using AiChat.Domain.Aggregates.ChannelAggregate;

namespace AiChat.Application.Services;

public class UsageReportService : IUsageReportService
{
    private readonly IUserRepository _userRepository;
    private readonly IUsageReportRepository _usageReportRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IGroupRepository _groupRepository;

    public UsageReportService(
        IUserRepository userRepository,
        IUsageReportRepository usageReportRepository,
        IChannelRepository channelRepository,
        IGroupRepository groupRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _usageReportRepository = usageReportRepository ?? throw new ArgumentNullException(nameof(usageReportRepository));
        _channelRepository = channelRepository ?? throw new ArgumentNullException(nameof(channelRepository));
        _groupRepository = groupRepository ?? throw new ArgumentNullException(nameof(groupRepository));
    }

    public async Task<UsageSummaryDto> GetUsageSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        Group? group = null;
        if (user.GroupId.HasValue)
        {
            group = await _groupRepository.GetByIdAsync(user.GroupId.Value, cancellationToken);
        }

        var planName = group?.Name ?? "Default";
        var monthlyLimit = group?.MonthlyTokenLimit ?? 0;

        // 获取本月用量
        // User 聚合根也存储了 CurrentMonthTotalTokens，可以直接用，减少数据库查询
        // 但为了准确性，也可以查 UsageReport 表。这里优先用 User 上的缓存字段。
        // 然而 User 上的字段需要确保同步更新。Assuming it is.
        // 为了稳妥，我们从 user 获取。

        return new UsageSummaryDto
        {
            CurrentMonthTotalTokens = user.CurrentMonthTotalTokens,
            MonthlyLimit = monthlyLimit,
            PlanName = planName
        };
    }

    public async Task<IEnumerable<DailyUsageDto>> GetDailyUsageAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-days + 1);

        var reports = await _usageReportRepository.GetUserReportsAsync(userId, startDate, endDate, cancellationToken);

        // 按日期聚合
        var grouped = reports
            .GroupBy(r => r.Date)
            .Select(g => new DailyUsageDto
            {
                Date = g.Key,
                InputTokens = g.Sum(r => r.InputTokens),
                OutputTokens = g.Sum(r => r.OutputTokens)
            })
            .OrderBy(d => d.Date)
            .ToList();

        // 填充缺失的日期
        var result = new List<DailyUsageDto>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var existing = grouped.FirstOrDefault(d => d.Date == date);
            result.Add(existing ?? new DailyUsageDto { Date = date, InputTokens = 0, OutputTokens = 0 });
        }

        return result;
    }

    public async Task<IEnumerable<ModelUsageDto>> GetModelUsageAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        // 默认查询本月
        var startDate = new DateOnly(today.Year, today.Month, 1);
        var endDate = today;

        var reports = await _usageReportRepository.GetUserReportsAsync(userId, startDate, endDate, cancellationToken);

        // 聚合模型ID
        var modelStats = reports
            .GroupBy(r => r.ModelId)
            .Select(g => new
            {
                ModelId = g.Key,
                TotalTokens = g.Sum(r => r.TotalTokens)
            })
            .OrderByDescending(x => x.TotalTokens)
            .ToList();

        // 获取模型名称
        var channels = await _channelRepository.GetAllAsync(cancellationToken);

        // Channel.Models 是 IReadOnlyCollection<AiModel>，需要确保能访问到
        var allModels = channels
            .Where(c => c.Models != null)
            .SelectMany(c => c.Models)
            .ToDictionary(m => m.Id, m => m.Name);

        var result = new List<ModelUsageDto>();
        foreach (var stat in modelStats)
        {
            var name = allModels.TryGetValue(stat.ModelId, out var n) ? n : "Unknown Model";
            result.Add(new ModelUsageDto
            {
                ModelId = stat.ModelId.ToString(),
                ModelName = name,
                TotalTokens = stat.TotalTokens
            });
        }

        return result;
    }
}
