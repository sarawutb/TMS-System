using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Vehicle : AuditableEntity
{
    public long VehicleId { get; set; }
    public string VehicleNo { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public long? CarrierId { get; set; }
    public decimal? CapacityWeightKg { get; set; }
    public decimal? CapacityVolumeM3 { get; set; }
    public bool TemperatureControlled { get; set; }
    public Carrier? Carrier { get; set; }
}
