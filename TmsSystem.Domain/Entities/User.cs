using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class User : AuditableEntity
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Email { get; set; }
    public long? RoleId { get; set; }
    public long? FactoryId { get; set; }
    public Role? Role { get; set; }
    public Factory? Factory { get; set; }
}
