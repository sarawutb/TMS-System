using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Driver : AuditableEntity
{
    public long DriverId { get; set; }
    public string DriverCode { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public long? CarrierId { get; set; }
    public string? MobileNo { get; set; }
    public string? LicenseNo { get; set; }
}
