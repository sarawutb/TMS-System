using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TmsSystem.BlazorWasm.Models;

namespace TmsSystem.BlazorWasm.Services;

public sealed class ApiAuthenticationStateProvider(LocalStorageService localStorage) : AuthenticationStateProvider
{
    private const string SessionKey = "tms.auth.session";
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var session = await GetSessionAsync();
        return new AuthenticationState(session is null ? Anonymous : CreatePrincipal(session));
    }

    public async Task<AuthUserSession?> GetSessionAsync()
    {
        var session = await localStorage.GetItemAsync<AuthUserSession>(SessionKey);
        if (session is null) return null;

        if (session.ExpiresAtUtc <= DateTime.UtcNow)
        {
            await localStorage.RemoveItemAsync(SessionKey);
            return null;
        }

        return session;
    }

    public async Task SignInAsync(AuthUserSession session)
    {
        await localStorage.SetItemAsync(SessionKey, session);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(CreatePrincipal(session))));
    }

    public async Task SignOutAsync()
    {
        await localStorage.RemoveItemAsync(SessionKey);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(Anonymous)));
    }

    private static ClaimsPrincipal CreatePrincipal(AuthUserSession session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new(ClaimTypes.Name, session.UserName),
            new("fullName", session.FullName),
            new("email", session.Email),
            new(ClaimTypes.Role, session.RoleName),
            new("roleCode", session.RoleCode),
            new("accessToken", session.AccessToken)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "JwtAuth"));
    }
}
