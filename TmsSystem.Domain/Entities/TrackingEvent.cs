using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class TrackingEvent : AuditableEntity
{
    public long TrackingEventId { get; set; }
    public long ShipmentId { get; set; }
    public long? DriverId { get; set; }
    public long? VehicleId { get; set; }
    public string EventCode { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public string? SafetyEventType { get; set; }
    public string? ExternalEventRef { get; set; }
    public string? Remark { get; set; }
}
