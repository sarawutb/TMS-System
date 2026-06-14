using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class IotDevice : AuditableEntity
{
    public long IotDeviceId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public long? VehicleId { get; set; }
    public long? CarrierId { get; set; }
}
