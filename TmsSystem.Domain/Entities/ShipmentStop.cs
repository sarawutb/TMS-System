using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class ShipmentStop : AuditableEntity
{
    public long ShipmentStopId { get; set; }
    public long ShipmentId { get; set; }
    public int StopNo { get; set; }
    public long StopProfileId { get; set; }
    public long LocationId { get; set; }
    public DateTime? PlannedArrivalDate { get; set; }
    public DateTime? ActualArrivalDate { get; set; }
    public DateTime? PlannedDepartureDate { get; set; }
    public DateTime? ActualDepartureDate { get; set; }
    public string? DockCode { get; set; }
    public string StopStatus { get; set; } = string.Empty;

    [NotMapped]
    public string StopType { get; set; } = string.Empty;
}

