using System.Net.Http.Json;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Auth;
using TmsSystem.BlazorWasm.Models;

namespace TmsSystem.BlazorWasm.Services;

public sealed class AuthSessionService(
    HttpClient httpClient,
    ApiAuthenticationStateProvider authenticationStateProvider)
{
    public async Task<OperationResult<AuthUserSession>> LoginAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/auth/login", new LoginRequestDto
            {
                UserName = userName,
                Password = password
            }, cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>(cancellationToken: cancellationToken);

            if (result is null || !result.Success || result.Data is null)
            {
                var error = result?.Message ?? "Invalid username or password.";
                if (result?.Errors is not null && result.Errors.Any())
                {
                    error = string.Join(" ", result.Errors);
                }
                return OperationResult<AuthUserSession>.Failure(error);
            }

            var session = new AuthUserSession
            {
                UserId = result.Data.UserId,
                UserName = result.Data.UserName,
                FullName = result.Data.FullName,
                RoleId = 0,
                RoleCode = result.Data.RoleCode,
                RoleName = result.Data.RoleName,
                AccessToken = result.Data.AccessToken,
                RefreshToken = result.Data.RefreshToken,
                ExpiresAtUtc = result.Data.AccessTokenExpiresAtUtc
            };

            await authenticationStateProvider.SignInAsync(session);
            return OperationResult<AuthUserSession>.Success(session, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<AuthUserSession>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        var currentSession = await authenticationStateProvider.GetSessionAsync();
        if (currentSession is not null)
        {
            try
            {
                await httpClient.PostAsJsonAsync("api/auth/logout", new LogoutRequestDto
                {
                    RefreshToken = currentSession.RefreshToken
                }, cancellationToken);
            }
            catch
            {
                // Soft logout: ignore network failures
            }
        }

        await authenticationStateProvider.SignOutAsync();
    }

    public Task<AuthUserSession?> GetCurrentSessionAsync() => authenticationStateProvider.GetSessionAsync();
}
