using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Auth;
using TmsSystem.Application.Interfaces;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;
using TmsSystem.Infrastructure.Security;

namespace TmsSystem.Infrastructure.Services;

public sealed class AuthService(TmsDbContext dbContext, IJwtTokenService jwtTokenService, IConfiguration configuration) : IAuthService
{
    private static readonly ConcurrentDictionary<string, RefreshTokenSession> RefreshTokens = new();

    public async Task<OperationResult<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username == request.UserName, cancellationToken);

        if (user is null || !user.IsActive || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return OperationResult<AuthResponseDto>.Failure("Invalid username or password.");
        }

        return OperationResult<AuthResponseDto>.Success(CreateTokenPair(user, user.Role), "Login successful.");
    }

    public async Task<OperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = PasswordHasher.Sha256Base64(request.RefreshToken);
        if (!RefreshTokens.TryRemove(tokenHash, out var session) || session.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return OperationResult<AuthResponseDto>.Failure("Invalid or expired refresh token.");
        }

        var user = await dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == session.UserId && x.IsActive, cancellationToken);

        if (user is null)
        {
            return OperationResult<AuthResponseDto>.Failure("Invalid or expired refresh token.");
        }

        return OperationResult<AuthResponseDto>.Success(CreateTokenPair(user, user.Role), "Token refreshed.");
    }

    public Task<OperationResult<bool>> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default)
    {
        var tokenHash = PasswordHasher.Sha256Base64(request.RefreshToken);
        RefreshTokens.TryRemove(tokenHash, out _);
        return Task.FromResult(OperationResult<bool>.Success(true, "Logout successful."));
    }

    public async Task<OperationResult<AuthenticatedUserDto>> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(x => x.Username == request.Username, cancellationToken))
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Username already exists.");
        }

        var role = request.RoleId is null
            ? null
            : await dbContext.Roles.FirstOrDefaultAsync(x => x.RoleId == request.RoleId && x.IsActive, cancellationToken);

        if (request.RoleId is not null && role is null)
        {
            return OperationResult<AuthenticatedUserDto>.Failure("Role not found.");
        }

        var user = new User
        {
            Username = request.Username,
            FullName = request.FullName,
            Email = request.Email,
            RoleId = request.RoleId,
            FactoryId = request.FactoryId,
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            CreateBy = "auth"
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<AuthenticatedUserDto>.Success(ToUserDto(user, role), "User registered.");
    }

    private AuthResponseDto CreateTokenPair(User user, Role? role)
    {
        var safeRole = role ?? new Role();
        var accessExpiresAt = DateTime.UtcNow.AddMinutes(GetInt("Jwt:AccessTokenMinutes", 30));
        var refreshExpiresAt = DateTime.UtcNow.AddDays(GetInt("Jwt:RefreshTokenDays", 7));
        var accessToken = jwtTokenService.CreateAccessToken(user, safeRole, accessExpiresAt);
        var refreshTokenValue = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

        RefreshTokens[PasswordHasher.Sha256Base64(refreshTokenValue)] = new RefreshTokenSession(user.UserId, refreshExpiresAt);

        return new AuthResponseDto
        {
            UserId = user.UserId,
            UserName = user.Username,
            FullName = user.FullName,
            RoleCode = safeRole.RoleCode,
            RoleName = safeRole.RoleName,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAtUtc = accessExpiresAt,
            RefreshTokenExpiresAtUtc = refreshExpiresAt
        };
    }

    private int GetInt(string key, int defaultValue) => int.TryParse(configuration[key], out var value) ? value : defaultValue;

    private static AuthenticatedUserDto ToUserDto(User user, Role? role) => new()
    {
        UserId = user.UserId,
        UserName = user.Username,
        FullName = user.FullName,
        RoleCode = role?.RoleCode ?? string.Empty,
        RoleName = role?.RoleName ?? string.Empty
    };

    private sealed record RefreshTokenSession(long UserId, DateTime ExpiresAtUtc);
}
