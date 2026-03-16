using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

/// <summary>
/// 系统配置服务接口
/// </summary>
public interface ISystemConfigService
{
    Task<SystemConfigDto> GetConfigAsync(CancellationToken cancellationToken = default);
    Task<PublicConfigDto> GetPublicConfigAsync(CancellationToken cancellationToken = default);
    Task<SystemConfigDto> UpdateConfigAsync(UpdateSystemConfigRequest request, CancellationToken cancellationToken = default);
}
