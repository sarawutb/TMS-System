using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class DriverPerformance : AuditableEntity
{
    public long DriverPerformanceId { get; set; }
    public long DriverId { get; set; }
    public string PeriodMonth { get; set; } = string.Empty;
    public decimal? OnTimeScore { get; set; }
    public decimal? SafetyScore { get; set; }
    public decimal? FuelEfficiencyScore { get; set; }
    public decimal? PodAccuracyScore { get; set; }
    public decimal? OverallScore { get; set; }
}
