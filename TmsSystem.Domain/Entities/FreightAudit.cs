using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class FreightAudit : AuditableEntity
{
    public long FreightAuditId { get; set; }
    public long ShipmentId { get; set; }
    public long CarrierId { get; set; }
    public string? CarrierInvoiceNo { get; set; }
    public decimal ExpectedAmount { get; set; }
    public decimal CarrierAmount { get; set; }
    public decimal DifferenceAmount { get; set; }
    public string AuditStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
}
