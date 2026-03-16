using AiChat.Application.DTOs;
using AiChat.Application.Interfaces;
using AiChat.Domain.Aggregates.UserAggregate;

namespace AiChat.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    // Refresh Token 有效期：7天
    private const int RefreshTokenExpirationDays = 7;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _jwtTokenGenerator = jwtTokenGenerator ?? throw new ArgumentNullException(nameof(jwtTokenGenerator));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // 检查用户名是否已存在
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser != null)
            throw new InvalidOperationException("Username already exists.");

        // 检查邮箱是否已存在
        var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail != null)
            throw new InvalidOperationException("Email already exists.");

        // 哈希密码
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 创建用户
        var user = new User(Guid.NewGuid(), request.Username, request.Email, passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 生成 Tokens
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // 查找用户
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("Invalid username or password.");

        // 验证密码
        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
            throw new InvalidOperationException("Invalid username or password.");

        // 检查用户是否被封禁
        if (user.IsBanned)
            throw new InvalidOperationException($"账户已被封禁：{user.BanReason}");

        // 生成 Tokens
        return await GenerateAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        // 查找 Refresh Token
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (refreshToken == null)
            throw new InvalidOperationException("Invalid refresh token.");

        if (!refreshToken.IsActive)
            throw new InvalidOperationException("Refresh token has expired or been revoked.");

        // 查找用户
        var user = await _userRepository.GetByIdAsync(refreshToken.UserId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        // 撤销旧的 Refresh Token（Token 轮换）
        var newRefreshToken = await RotateRefreshTokenAsync(refreshToken, user.Id, cancellationToken);

        // 生成新的 Access Token（包含角色）
        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Role);

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
            return;

        token.Revoke();
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        // 生成 Access Token（包含角色信息）
        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateToken(user.Id, user.Username, user.Role);

        // 生成 Refresh Token
        var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var refreshToken = new RefreshToken(
            Guid.NewGuid(),
            refreshTokenValue,
            user.Id,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)
        );

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            }
        };
    }

    private async Task<RefreshToken> RotateRefreshTokenAsync(
        RefreshToken oldToken,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // 生成新的 Refresh Token
        var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken(
            Guid.NewGuid(),
            newRefreshTokenValue,
            userId,
            DateTime.UtcNow.AddDays(RefreshTokenExpirationDays)
        );

        // 撤销旧 Token 并关联新 Token
        oldToken.Revoke(newRefreshToken.Id);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        return newRefreshToken;
    }

    // === 用户管理功能 ===

    public async Task BanUserAsync(Guid userId, string reason, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Ban(reason);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UnbanUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        user.Unban();
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatePassword(passwordHash);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
