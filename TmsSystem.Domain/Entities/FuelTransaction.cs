using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class FuelTransaction : AuditableEntity
{
    public long FuelTransactionId { get; set; }
    public long VehicleId { get; set; }
    public long? DriverId { get; set; }
    public long? ShipmentId { get; set; }
    public DateTime FuelDate { get; set; }
    public decimal FuelLiter { get; set; }
    public decimal FuelCost { get; set; }
    public decimal? OdometerKm { get; set; }
    public string? StationName { get; set; }
}
