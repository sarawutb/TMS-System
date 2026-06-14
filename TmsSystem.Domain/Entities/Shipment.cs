using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Shipment : AuditableEntity
{
    public long ShipmentId { get; set; }
    public string ShipmentNo { get; set; } = string.Empty;
    public long? TransportOrderId { get; set; }
    public long? RoutePlanId { get; set; }
    public long? LoadPlanId { get; set; }
    public long? CarrierId { get; set; }
    public long? VehicleId { get; set; }
    public long? DriverId { get; set; }
    public string ShipmentStatus { get; set; } = string.Empty;
    public DateTime? PlannedPickupDate { get; set; }
    public DateTime? PlannedDeliveryDate { get; set; }
    public DateTime? ActualPickupDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public DateTime? LatestEta { get; set; }
}
