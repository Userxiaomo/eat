using AiChat.Domain.Aggregates.ConversationAggregate;
using AiChat.Domain.Aggregates.UserAggregate;
using AiChat.Domain.Aggregates.ChannelAggregate;
using AiChat.Domain.Aggregates.BotAggregate;
using AiChat.Domain.Aggregates.SearchAggregate;
using AiChat.Domain.Aggregates.McpAggregate;
using AiChat.Domain.Aggregates.SystemAggregate;
using Microsoft.EntityFrameworkCore;

namespace AiChat.Infrastructure.Persistence;

public class AiChatDbContext : DbContext
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Channel> Channels => Set<Channel>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupModelMapping> GroupModelMappings => Set<GroupModelMapping>();
    public DbSet<UsageReport> UsageReports => Set<UsageReport>();
    public DbSet<Bot> Bots => Set<Bot>();
    public DbSet<SearchEngineConfig> SearchEngineConfigs => Set<SearchEngineConfig>();
    public DbSet<McpServer> McpServers => Set<McpServer>();
    public DbSet<McpTool> McpTools => Set<McpTool>();
    public DbSet<ModelMapping> ModelMappings => Set<ModelMapping>();
    public DbSet<SharedConversation> SharedConversations => Set<SharedConversation>();
    public DbSet<SystemConfiguration> SystemConfigurations => Set<SystemConfiguration>();

    public AiChatDbContext(DbContextOptions<AiChatDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiChatDbContext).Assembly);
    }
}
