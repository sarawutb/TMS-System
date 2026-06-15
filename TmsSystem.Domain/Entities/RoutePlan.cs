using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class RoutePlan : AuditableEntity
{
    public long RoutePlanId { get; set; }
    public string RoutePlanNo { get; set; } = string.Empty;
    public DateTime PlanDate { get; set; }
    public string? OptimizationEngine { get; set; }
    public string? TransportMode { get; set; }
    public string? VehicleType { get; set; }
    public long? VehicleId { get; set; }
    public long? CarrierId { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public int? TotalDurationMinute { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? RiskScore { get; set; }
    public DateTime? EarliestPickupWindow { get; set; }
    public DateTime? LatestDeliveryWindow { get; set; }
    public string? StopSequenceJson { get; set; }
    public string? ComplianceIssuesJson { get; set; }
    public string? SolverMetadataJson { get; set; }
    public string Status { get; set; } = string.Empty;
    public Vehicle? Vehicle { get; set; }
    public Carrier? Carrier { get; set; }
    public ICollection<LoadPlan> LoadPlans { get; set; } = new List<LoadPlan>();
}
