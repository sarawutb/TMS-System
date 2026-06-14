using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class VehicleMaintenance : AuditableEntity
{
    public long VehicleMaintenanceId { get; set; }
    public long VehicleId { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public DateTime ScheduleDate { get; set; }
    public DateTime? CompleteDate { get; set; }
    public decimal? OdometerKm { get; set; }
    public string MaintenanceStatus { get; set; } = string.Empty;
    public string? Remark { get; set; }
}
