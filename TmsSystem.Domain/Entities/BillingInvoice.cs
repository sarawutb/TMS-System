using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class BillingInvoice : AuditableEntity
{
    public long BillingInvoiceId { get; set; }
    public long ShipmentId { get; set; }
    public long? CustomerId { get; set; }
    public string InvoiceNo { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public decimal InvoiceAmount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string InvoiceStatus { get; set; } = string.Empty;
    public string? ErpPostingStatus { get; set; }
}
