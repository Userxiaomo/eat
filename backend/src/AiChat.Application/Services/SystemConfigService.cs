using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.SystemAggregate;

namespace AiChat.Application.Services;

public class SystemConfigService : ISystemConfigService
{
    private readonly ISystemConfigRepository _configRepository;

    public SystemConfigService(ISystemConfigRepository configRepository)
    {
        _configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));
    }

    public async Task<SystemConfigDto> GetConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.GetOrCreateAsync(cancellationToken);

        return new SystemConfigDto
        {
            SiteName = config.SiteName,
            SiteLogo = config.SiteLogo,
            Announcement = config.Announcement,
            ContactInfo = config.ContactInfo,
            EnableRegistration = config.EnableRegistration,
            EnableEmailVerification = config.EnableEmailVerification,
            DefaultGroupId = config.DefaultGroupId,
            UpdatedAt = config.UpdatedAt
        };
    }

    public async Task<PublicConfigDto> GetPublicConfigAsync(CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.GetOrCreateAsync(cancellationToken);

        return new PublicConfigDto
        {
            SiteName = config.SiteName,
            SiteLogo = config.SiteLogo,
            Announcement = config.Announcement,
            EnableRegistration = config.EnableRegistration
        };
    }

    public async Task<SystemConfigDto> UpdateConfigAsync(UpdateSystemConfigRequest request, CancellationToken cancellationToken = default)
    {
        var config = await _configRepository.GetOrCreateAsync(cancellationToken);

        config.UpdateBasicInfo(request.SiteName, request.SiteLogo, request.Announcement, request.ContactInfo);
        config.SetRegistrationEnabled(request.EnableRegistration);
        config.SetEmailVerificationEnabled(request.EnableEmailVerification);
        config.SetDefaultGroup(request.DefaultGroupId);

        _configRepository.Update(config);
        await _configRepository.SaveChangesAsync(cancellationToken);

        return new SystemConfigDto
        {
            SiteName = config.SiteName,
            SiteLogo = config.SiteLogo,
            Announcement = config.Announcement,
            ContactInfo = config.ContactInfo,
            EnableRegistration = config.EnableRegistration,
            EnableEmailVerification = config.EnableEmailVerification,
            DefaultGroupId = config.DefaultGroupId,
            UpdatedAt = config.UpdatedAt
        };
    }
}
