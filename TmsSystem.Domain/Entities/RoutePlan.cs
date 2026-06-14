using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class RoutePlan : AuditableEntity
{
    public long RoutePlanId { get; set; }
    public string RoutePlanNo { get; set; } = string.Empty;
    public DateTime PlanDate { get; set; }
    public string? OptimizationEngine { get; set; }
    public decimal? TotalDistanceKm { get; set; }
    public int? TotalDurationMinute { get; set; }
    public decimal? EstimatedCost { get; set; }
    public decimal? RiskScore { get; set; }
    public string Status { get; set; } = string.Empty;
}
