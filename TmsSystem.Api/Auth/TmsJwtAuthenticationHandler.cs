using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TmsSystem.Api.Responses;
using TmsSystem.Application.Interfaces;

namespace TmsSystem.Api.Auth;

public sealed class TmsJwtAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IJwtTokenService jwtTokenService) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return Task.FromResult(AuthenticateResult.NoResult());
        var principal = jwtTokenService.ValidateAccessToken(authorization["Bearer ".Length..].Trim());
        return Task.FromResult(principal is null ? AuthenticateResult.Fail("Invalid bearer token.") : AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        => WriteResponseAsync(StatusCodes.Status401Unauthorized, "Unauthorized.");

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        => WriteResponseAsync(StatusCodes.Status403Forbidden, "Forbidden.");

    private async Task WriteResponseAsync(int statusCode, string message)
    {
        Response.StatusCode = statusCode;
        Response.ContentType = "application/json";

        var response = ApiResponse<object>.FailureResponse(statusCode, message);
        await Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
}
