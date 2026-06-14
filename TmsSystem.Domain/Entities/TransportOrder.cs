using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class TransportOrder : AuditableEntity
{
    public long TransportOrderId { get; set; }
    public string TransportOrderNo { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string? SourceDocumentNo { get; set; }
    public long FactoryId { get; set; }
    public long? CustomerId { get; set; }
    public long PickupLocationId { get; set; }
    public long DeliveryLocationId { get; set; }
    public DateTime? RequestedPickupDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string Priority { get; set; } = string.Empty;
    public bool TemperatureRequired { get; set; }
    public string Status { get; set; } = string.Empty;
}
