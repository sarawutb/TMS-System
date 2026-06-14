using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Auth;

namespace TmsSystem.Application.Interfaces;

public interface IAuthService
{
    Task<OperationResult<AuthResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<OperationResult<AuthResponseDto>> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<OperationResult<bool>> LogoutAsync(LogoutRequestDto request, CancellationToken cancellationToken = default);
    Task<OperationResult<AuthenticatedUserDto>> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken = default);
}
