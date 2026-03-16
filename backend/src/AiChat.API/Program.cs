using System.Text;
using AiChat.API.Hubs;
using AiChat.API.Middleware;
using AiChat.Application.Interfaces;
using AiChat.Application.Services;
using AiChat.Application.Validators;
using AiChat.Domain.Aggregates.ConversationAggregate;
using AiChat.Domain.Aggregates.UserAggregate;
using AiChat.Domain.Aggregates.ChannelAggregate;
using AiChat.Domain.Aggregates.BotAggregate;
using AiChat.Domain.Aggregates.SearchAggregate;
using AiChat.Domain.Aggregates.McpAggregate;
using AiChat.Domain.Aggregates.SystemAggregate;
using AiChat.Infrastructure.AI;
using AiChat.Infrastructure.Identity;
using AiChat.Infrastructure.Persistence;
using AiChat.Infrastructure.Persistence.Repositories;
using AiChat.Infrastructure.Security;
using AiChat.Infrastructure.Search;
using AspNetCoreRateLimit;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration
builder.Services.AddDbContext<AiChatDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IConversationRepository, ConversationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IChannelRepository, ChannelRepository>();
builder.Services.AddScoped<IAiModelRepository, AiModelRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IUsageReportRepository, UsageReportRepository>();
builder.Services.AddScoped<IBotRepository, BotRepository>();
builder.Services.AddScoped<ISearchEngineConfigRepository, SearchEngineConfigRepository>();
builder.Services.AddScoped<IMcpServerRepository, McpServerRepository>();
builder.Services.AddScoped<IModelMappingRepository, ModelMappingRepository>();  // 模型映射仓储
builder.Services.AddScoped<ISharedConversationRepository, SharedConversationRepository>();  // 对话分享仓储
builder.Services.AddScoped<ISystemConfigRepository, SystemConfigRepository>();  // 系统配置仓储

// Security Services
builder.Services.AddSingleton<IEncryptionService, AesEncryptionService>();

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IChannelService, ChannelService>();
builder.Services.AddScoped<IAiModelService, AiModelService>();
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IUsageReportService, UsageReportService>();
builder.Services.AddScoped<IBotService, BotService>();
builder.Services.AddScoped<IChannelLoadBalancer, ChannelLoadBalancer>();  // 渠道负载均衡器
builder.Services.AddScoped<ISystemConfigService, SystemConfigService>();  // 系统配置服务
builder.Services.AddScoped<IAnalyticsService, AiChat.Infrastructure.Services.AnalyticsService>();  // 统计分析服务

// 后台服务
builder.Services.AddHostedService<AiChat.Infrastructure.BackgroundServices.ChannelHealthCheckService>();


// 搜索服务注册
builder.Services.AddScoped<TavilySearchService>();
builder.Services.AddScoped<JinaSearchService>();
builder.Services.AddScoped<IWebSearchServiceFactory, WebSearchServiceFactory>();
// 默认注入 Tavily (如果还有地方直接使用 IWebSearchService)
builder.Services.AddScoped<IWebSearchService>(sp => sp.GetRequiredService<TavilySearchService>());
builder.Services.AddScoped<IMcpService, McpService>();

// HttpClient for external APIs
builder.Services.AddHttpClient("Tavily", client =>
{
    client.BaseAddress = new Uri("https://api.tavily.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// AI Services - Semantic Kernel
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ?? "your-openai-api-key-here";
var openAiModelId = builder.Configuration["OpenAI:ModelId"] ?? "gpt-4o-mini";

// 只有在 API Key 有效时才注册 Semantic Kernel
if (!string.IsNullOrEmpty(openAiApiKey) && openAiApiKey != "your-openai-api-key-here")
{
    builder.Services.AddKernel()
        .AddOpenAIChatCompletion(
            modelId: openAiModelId,
            apiKey: openAiApiKey);
}
else
{
    // 未配置 API Key 时使用空 Kernel（OpenAiService 会自动切换到模拟模式）
    builder.Services.AddKernel();
}

// 注册 AI 服务
builder.Services.AddScoped<OpenAiService>();
builder.Services.AddScoped<ClaudeService>();
builder.Services.AddScoped<DynamicAiService>();
builder.Services.AddScoped<AiServiceFactory>();

// 保留向后兼容（如果有代码直接注入 IAiService，默认使用动态服务）
builder.Services.AddScoped<IAiService, DynamicAiService>();

// JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "AiChatAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "AiChatClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// SignalR
builder.Services.AddSignalR();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/register",
            Period = "1m",
            Limit = 3  // 每分钟3次注册请求
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1m",
            Limit = 5  // 每分钟5次登录请求
        },
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 60  // 其他端点每分钟60次
        }
    };
});
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

var app = builder.Build();

// Global Exception Handler (should be first)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseIpRateLimiting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR Hub
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
