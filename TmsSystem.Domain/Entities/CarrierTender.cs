using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class CarrierTender : AuditableEntity
{
    public long CarrierTenderId { get; set; }
    public long RoutePlanId { get; set; }
    public long CarrierId { get; set; }
    public string TenderNo { get; set; } = string.Empty;
    public DateTime TenderDate { get; set; }
    public string TenderStatus { get; set; } = string.Empty;
    public decimal? OfferedCost { get; set; }
    public DateTime? ResponseDate { get; set; }
    public string? RejectReason { get; set; }
}
