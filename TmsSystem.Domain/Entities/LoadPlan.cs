using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class LoadPlan : AuditableEntity
{
    public long LoadPlanId { get; set; }
    public long? RoutePlanId { get; set; }
    public string LoadPlanNo { get; set; } = string.Empty;
    public long VehicleProfileId { get; set; }
    public long? VehicleId { get; set; }
    public decimal? ContainerLengthM { get; set; }
    public decimal? ContainerWidthM { get; set; }
    public decimal? ContainerHeightM { get; set; }
    public decimal? CapacityWeightKg { get; set; }
    public decimal? CapacityVolumeM3 { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public decimal? TotalVolumeM3 { get; set; }
    public decimal? UtilizationPercent { get; set; }
    public string? ThreeDPlanRef { get; set; }
    public string? LoadPlanJson { get; set; }
    public string? PlacementJson { get; set; }
    public string? ConstraintIssuesJson { get; set; }
    public bool LoadFeasible { get; set; } = true;
    public RoutePlan? RoutePlan { get; set; }
    public Vehicle? Vehicle { get; set; }

    [NotMapped]
    public string VehicleType { get; set; } = string.Empty;
}

