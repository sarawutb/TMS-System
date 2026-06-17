using System.ComponentModel.DataAnnotations.Schema;
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
    public long SourceProfileId { get; set; }
    public long? SafetyEventProfileId { get; set; }
    public string? ExternalEventRef { get; set; }
    public string? Remark { get; set; }

    [NotMapped]
    public string SourceType { get; set; } = string.Empty;

    [NotMapped]
    public string? SafetyEventType { get; set; }
}

