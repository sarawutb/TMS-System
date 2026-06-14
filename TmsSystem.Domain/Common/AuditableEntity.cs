namespace TmsSystem.Domain.Common;

public abstract class AuditableEntity
{
    public bool IsActive { get; set; } = true;
    public string CreateBy { get; set; } = "system";
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public string? ReviseBy { get; set; }
    public DateTime? ReviseDate { get; set; }
}
