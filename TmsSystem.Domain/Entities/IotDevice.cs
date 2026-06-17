using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class IotDevice : AuditableEntity
{
    public long IotDeviceId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public long DeviceProfileId { get; set; }
    public long? VehicleId { get; set; }
    public long? CarrierId { get; set; }

    [NotMapped]
    public string DeviceType { get; set; } = string.Empty;
}

