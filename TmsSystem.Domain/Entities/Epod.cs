using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Epod : AuditableEntity
{
    public long EpodId { get; set; }
    public long ShipmentId { get; set; }
    public long DeliveryLocationId { get; set; }
    public string? ReceivedBy { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string? SignatureFileRef { get; set; }
    public string? PhotoFileRef { get; set; }
    public string PodStatus { get; set; } = string.Empty;
    public string? Remark { get; set; }
}
