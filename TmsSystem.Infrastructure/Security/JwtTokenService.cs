using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TmsSystem.Application.Interfaces;
using TmsSystem.Domain.Entities;

namespace TmsSystem.Infrastructure.Security;

public sealed class JwtTokenService(IConfiguration configuration) : IJwtTokenService
{
    public string CreateAccessToken(User user, Role role, DateTime expiresAtUtc)
    {
        var header = new Dictionary<string, object> { ["alg"] = "HS256", ["typ"] = "JWT" };
        var payload = new Dictionary<string, object> { ["sub"] = user.UserId.ToString(), ["name"] = user.Username, ["fullName"] = user.FullName, ["role"] = role.RoleCode, ["roleName"] = role.RoleName, ["jti"] = Guid.NewGuid().ToString("N"), ["iat"] = ToUnixTime(DateTime.UtcNow), ["exp"] = ToUnixTime(expiresAtUtc) };
        var unsignedToken = $"{Base64Url(JsonSerializer.SerializeToUtf8Bytes(header))}.{Base64Url(JsonSerializer.SerializeToUtf8Bytes(payload))}";
        return $"{unsignedToken}.{Base64Url(Sign(unsignedToken))}";
    }
    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var parts = token.Split('.'); if (parts.Length != 3) return null;
        var unsignedToken = $"{parts[0]}.{parts[1]}"; var expectedSignature = Base64Url(Sign(unsignedToken));
        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expectedSignature), Encoding.ASCII.GetBytes(parts[2]))) return null;
        using var payloadDoc = JsonDocument.Parse(Base64UrlDecode(parts[1])); var payload = payloadDoc.RootElement;
        if (!payload.TryGetProperty("exp", out var expElement) || DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64()).UtcDateTime <= DateTime.UtcNow) return null;
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, payload.GetProperty("sub").GetString() ?? string.Empty), new(ClaimTypes.Name, payload.GetProperty("name").GetString() ?? string.Empty), new(ClaimTypes.Role, payload.GetProperty("role").GetString() ?? string.Empty), new("fullName", payload.GetProperty("fullName").GetString() ?? string.Empty), new("roleName", payload.GetProperty("roleName").GetString() ?? string.Empty) };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TmsJwt"));
    }
    private byte[] Sign(string value){ using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? "TmsSystemDevelopmentSecretChangeMeToAtLeast32Characters")); return hmac.ComputeHash(Encoding.ASCII.GetBytes(value)); }
    private static long ToUnixTime(DateTime value) => new DateTimeOffset(value).ToUnixTimeSeconds();
    private static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static byte[] Base64UrlDecode(string value){ var padded=value.Replace('-', '+').Replace('_', '/'); padded=padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '='); return Convert.FromBase64String(padded); }
}
