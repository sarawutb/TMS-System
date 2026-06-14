namespace TmsSystem.Application.Dtos.Auth;

public sealed class AuthenticatedUserDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
