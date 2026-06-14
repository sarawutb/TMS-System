using System.Security.Claims;
using TmsSystem.Domain.Entities;

namespace TmsSystem.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateAccessToken(User user, Role role, DateTime expiresAtUtc);
    ClaimsPrincipal? ValidateAccessToken(string token);
}
