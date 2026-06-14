using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class LoadPlan : AuditableEntity
{
    public long LoadPlanId { get; set; }
    public long? RoutePlanId { get; set; }
    public string LoadPlanNo { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public decimal? TotalWeightKg { get; set; }
    public decimal? TotalVolumeM3 { get; set; }
    public decimal? UtilizationPercent { get; set; }
    public string? ThreeDPlanRef { get; set; }
    public bool LoadFeasible { get; set; }
}
