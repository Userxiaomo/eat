using AiChat.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence.Repositories;

public class UsageReportRepository : IUsageReportRepository
{
    private readonly AiChatDbContext _context;

    public UsageReportRepository(AiChatDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UsageReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.UsageReports.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<UsageReport?> GetByUserDateModelAsync(
        Guid userId,
        DateOnly date,
        Guid modelId,
        Guid providerId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageReports
            .FirstOrDefaultAsync(r =>
                r.UserId == userId &&
                r.Date == date &&
                r.ModelId == modelId &&
                r.ProviderId == providerId,
                cancellationToken);
    }

    public async Task<int> GetUserMonthlyTotalTokensAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        return await _context.UsageReports
            .Where(r => r.UserId == userId && r.Date >= startDate && r.Date <= endDate)
            .SumAsync(r => r.InputTokens + r.OutputTokens, cancellationToken);
    }

    public async Task<IEnumerable<UsageReport>> GetUserReportsAsync(
        Guid userId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.UsageReports
            .Where(r => r.UserId == userId && r.Date >= startDate && r.Date <= endDate)
            .OrderByDescending(r => r.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UsageReport report, CancellationToken cancellationToken = default)
    {
        await _context.UsageReports.AddAsync(report, cancellationToken);
    }

    public void Update(UsageReport report)
    {
        _context.UsageReports.Update(report);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
