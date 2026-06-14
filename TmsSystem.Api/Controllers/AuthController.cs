using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsSystem.Application.Dtos.Auth;
using TmsSystem.Application.Interfaces;

namespace TmsSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : TmsControllerBase
{
    [HttpPost("login")][AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await authService.LoginAsync(request, cancellationToken), failureCode: StatusCodes.Status401Unauthorized);

    [HttpPost("refresh")][AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await authService.RefreshAsync(request, cancellationToken), failureCode: StatusCodes.Status401Unauthorized);

    [HttpPost("logout")][AllowAnonymous]
    public async Task<IActionResult> Logout(LogoutRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await authService.LogoutAsync(request, cancellationToken));

    [HttpPost("register")][Authorize(Roles = "Administrator")]
    public async Task<IActionResult> Register(RegisterUserRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await authService.RegisterAsync(request, cancellationToken));

    [HttpGet("me")][Authorize]
    public IActionResult Me() => ApiSuccess(new { UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, UserName = User.Identity?.Name, Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value, FullName = User.FindFirst("fullName")?.Value, RoleName = User.FindFirst("roleName")?.Value });
}
