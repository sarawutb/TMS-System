namespace TmsSystem.Application.Dtos.Auth;

public sealed class RegisterUserRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public long? RoleId { get; set; }
    public long? FactoryId { get; set; }
}
