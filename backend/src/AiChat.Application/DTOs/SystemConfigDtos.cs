namespace AiChat.Application.DTOs;

// ========== 系统配置 DTOs ==========

public record SystemConfigDto
{
    public string SiteName { get; init; } = string.Empty;
    public string? SiteLogo { get; init; }
    public string? Announcement { get; init; }
    public string? ContactInfo { get; init; }
    public bool EnableRegistration { get; init; }
    public bool EnableEmailVerification { get; init; }
    public Guid? DefaultGroupId { get; init; }
    public DateTime UpdatedAt { get; init; }
}

public record UpdateSystemConfigRequest
{
    public string SiteName { get; init; } = string.Empty;
    public string? SiteLogo { get; init; }
    public string? Announcement { get; init; }
    public string? ContactInfo { get; init; }
    public bool EnableRegistration { get; init; }
    public bool EnableEmailVerification { get; init; }
    public Guid? DefaultGroupId { get; init; }
}

/// <summary>
/// 公开配置（无需登录可访问）
/// </summary>
public record PublicConfigDto
{
    public string SiteName { get; init; } = string.Empty;
    public string? SiteLogo { get; init; }
    public string? Announcement { get; init; }
    public bool EnableRegistration { get; init; }
}
