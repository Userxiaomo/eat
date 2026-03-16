using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AiChatDbContext _context;

    public AnalyticsService(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<SystemOverviewDto> GetSystemOverviewAsync(CancellationToken cancellationToken = default)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);

        // 本月有Token使用的用户视为活跃用户
        var now = DateTime.UtcNow;
        var activeUsers = await _context.Users
            .Where(u => u.UsageUpdatedAt.Year == now.Year && u.UsageUpdatedAt.Month == now.Month)
            .CountAsync(cancellationToken);

        var totalConversations = await _context.Conversations.CountAsync(cancellationToken);
        var totalMessages = await _context.Messages.CountAsync(cancellationToken);

        // 统计总Token使用量（当月）
        var totalTokens = await _context.Users
            .Where(u => u.UsageUpdatedAt.Year == now.Year && u.UsageUpdatedAt.Month == now.Month)
            .SumAsync(u => u.CurrentMonthTotalTokens, cancellationToken);

        return new SystemOverviewDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            TotalConversations = totalConversations,
            TotalMessages = totalMessages,
            TotalTokensUsed = totalTokens
        };
    }

    public async Task<IEnumerable<ModelUsageDto>> GetModelUsageStatsAsync(CancellationToken cancellationToken = default)
    {
        // 按ModelId分组统计
        var stats = await _context.Conversations
            .GroupBy(c => c.ModelId)
            .Select(g => new
            {
                ModelId = g.Key,
                ConversationCount = g.Count(),
                MessageCount = g.SelectMany(c => c.Messages).Count(),
                TotalInputTokens = g.SelectMany(c => c.Messages).Sum(m => m.InputTokens ?? 0),
                TotalOutputTokens = g.SelectMany(c => c.Messages).Sum(m => m.OutputTokens ?? 0)
            })
            .ToListAsync(cancellationToken);

        return stats.Select(s => new ModelUsageDto
        {
            ModelId = s.ModelId,
            ConversationCount = s.ConversationCount,
            MessageCount = s.MessageCount,
            TotalInputTokens = s.TotalInputTokens,
            TotalOutputTokens = s.TotalOutputTokens
        });
    }

    public async Task<IEnumerable<UserTokenUsageDto>> GetTopUsersByTokenUsageAsync(int top = 10, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var topUsers = await _context.Users
            .Where(u => u.UsageUpdatedAt.Year == now.Year && u.UsageUpdatedAt.Month == now.Month)
            .OrderByDescending(u => u.CurrentMonthTotalTokens)
            .Take(top)
            .Select(u => new UserTokenUsageDto
            {
                UserId = u.Id,
                Username = u.Username,
                CurrentMonthTokens = u.CurrentMonthTotalTokens,
                UsageUpdatedAt = u.UsageUpdatedAt
            })
            .ToListAsync(cancellationToken);

        return topUsers;
    }

    public async Task<IEnumerable<GroupStatsDto>> GetGroupStatsAsync(CancellationToken cancellationToken = default)
    {
        // 统计每个分组的用户数和对话数
        var groupStats = await _context.Users
            .GroupBy(u => u.GroupId)
            .Select(g => new
            {
                GroupId = g.Key,
                UserCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        var result = new List<GroupStatsDto>();
        foreach (var stat in groupStats)
        {
            string? groupName = null;
            int conversationCount = 0;

            if (stat.GroupId.HasValue)
            {
                var group = await _context.Groups.FindAsync(new object[] { stat.GroupId.Value }, cancellationToken);
                groupName = group?.Name;

                // 统计该分组所有用户的对话总数
                var userIds = await _context.Users
                    .Where(u => u.GroupId == stat.GroupId)
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);
                conversationCount = await _context.Conversations
                    .Where(c => userIds.Contains(c.UserId))
                    .CountAsync(cancellationToken);
            }
            else
            {
                groupName = "未分组";
                var userIds = await _context.Users
                    .Where(u => u.GroupId == null)
                    .Select(u => u.Id)
                    .ToListAsync(cancellationToken);
                conversationCount = await _context.Conversations
                    .Where(c => userIds.Contains(c.UserId))
                    .CountAsync(cancellationToken);
            }

            result.Add(new GroupStatsDto
            {
                GroupId = stat.GroupId,
                GroupName = groupName,
                UserCount = stat.UserCount,
                ConversationCount = conversationCount
            });
        }

        return result;
    }
}
