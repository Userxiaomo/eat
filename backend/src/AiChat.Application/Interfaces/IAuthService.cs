using AiChat.Application.DTOs;

namespace AiChat.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);

    // 用户管理功能（需要Admin权限）
    Task BanUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default);
    Task UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateUserPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);
}
