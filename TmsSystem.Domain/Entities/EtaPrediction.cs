using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class EtaPrediction : AuditableEntity
{
    public long EtaPredictionId { get; set; }
    public long ShipmentId { get; set; }
    public DateTime PredictionDate { get; set; }
    public DateTime PredictedEta { get; set; }
    public decimal? ConfidenceScore { get; set; }
    public decimal? DelayRiskScore { get; set; }
    public string? ModelVersion { get; set; }
    public string? ReasonSummary { get; set; }
}
