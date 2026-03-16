using AiChat.Domain.Aggregates.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiChat.Infrastructure.Persistence.Configurations;

public class GroupModelMappingConfiguration : IEntityTypeConfiguration<GroupModelMapping>
{
    public void Configure(EntityTypeBuilder<GroupModelMapping> builder)
    {
        builder.ToTable("GroupModelMappings");

        // 联合主键
        builder.HasKey(m => new { m.GroupId, m.ModelId });
    }
}
