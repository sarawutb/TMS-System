using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Role : AuditableEntity
{
    public long RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Permissions { get; set; }
}
