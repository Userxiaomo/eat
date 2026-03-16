using AiChat.Domain.Common;

namespace AiChat.Domain.Aggregates.SystemAggregate;

/// <summary>
/// 系统配置聚合根
/// </summary>
public class SystemConfiguration : AggregateRoot<Guid>
{
    /// <summary>
    /// 站点名称
    /// </summary>
    public string SiteName { get; private set; } = "AiChat";

    /// <summary>
    /// 站点Logo URL
    /// </summary>
    public string? SiteLogo { get; private set; }

    /// <summary>
    /// 公告内容
    /// </summary>
    public string? Announcement { get; private set; }

    /// <summary>
    /// 联系方式
    /// </summary>
    public string? ContactInfo { get; private set; }

    /// <summary>
    /// 是否允许注册
    /// </summary>
    public bool EnableRegistration { get; private set; } = true;

    /// <summary>
    /// 是否启用邮箱验证
    /// </summary>
    public bool EnableEmailVerification { get; private set; } = false;

    /// <summary>
    /// 默认用户分组ID
    /// </summary>
    public Guid? DefaultGroupId { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // EF Core 需要
    private SystemConfiguration() { }

    public SystemConfiguration(Guid id) : base(id)
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 更新基本配置
    /// </summary>
    public void UpdateBasicInfo(string siteName, string? siteLogo, string? announcement, string? contactInfo)
    {
        if (string.IsNullOrWhiteSpace(siteName))
            throw new ArgumentException("SiteName cannot be empty.", nameof(siteName));

        SiteName = siteName;
        SiteLogo = siteLogo;
        Announcement = announcement;
        ContactInfo = contactInfo;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置注册开关
    /// </summary>
    public void SetRegistrationEnabled(bool enabled)
    {
        EnableRegistration = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置邮箱验证开关
    /// </summary>
    public void SetEmailVerificationEnabled(bool enabled)
    {
        EnableEmailVerification = enabled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 设置默认分组
    /// </summary>
    public void SetDefaultGroup(Guid? groupId)
    {
        DefaultGroupId = groupId;
        UpdatedAt = DateTime.UtcNow;
    }
}
