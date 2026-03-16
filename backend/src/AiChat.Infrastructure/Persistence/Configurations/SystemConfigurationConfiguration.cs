using AiChat.Domain.Aggregates.SystemAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class SystemConfigurationConfiguration : IEntityTypeConfiguration<SystemConfiguration>
{
    public void Configure(EntityTypeBuilder<SystemConfiguration> builder)
    {
        builder.ToTable("SystemConfigurations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SiteName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.SiteLogo)
            .HasMaxLength(500);

        builder.Property(s => s.Announcement)
            .HasColumnType("text");

        builder.Property(s => s.ContactInfo)
            .HasMaxLength(500);

        builder.Property(s => s.EnableRegistration)
            .IsRequired();

        builder.Property(s => s.EnableEmailVerification)
            .IsRequired();

        builder.Property(s => s.DefaultGroupId);

        builder.Property(s => s.UpdatedAt)
            .IsRequired();
    }
}
