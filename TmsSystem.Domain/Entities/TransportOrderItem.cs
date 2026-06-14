using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class TransportOrderItem : AuditableEntity
{
    public long TransportOrderItemId { get; set; }
    public long TransportOrderId { get; set; }
    public long? ProductId { get; set; }
    public int ItemNo { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductDescription { get; set; }
    public decimal Quantity { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public decimal? WeightKg { get; set; }
    public decimal? VolumeM3 { get; set; }
}
