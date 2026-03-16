using AiChat.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class UsageReportConfiguration : IEntityTypeConfiguration<UsageReport>
{
    public void Configure(EntityTypeBuilder<UsageReport> builder)
    {
        builder.ToTable("UsageReports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Date)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ModelId)
            .IsRequired();

        builder.Property(r => r.ProviderId)
            .IsRequired();

        builder.Property(r => r.InputTokens)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.OutputTokens)
            .IsRequired()
            .HasDefaultValue(0);

        // 忽略计算属性
        builder.Ignore(r => r.TotalTokens);

        // 创建索引优化查询
        builder.HasIndex(r => new { r.UserId, r.Date });
        builder.HasIndex(r => new { r.UserId, r.Date, r.ModelId, r.ProviderId }).IsUnique();
    }
}
