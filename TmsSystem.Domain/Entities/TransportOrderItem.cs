using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class TransportOrderItem : AuditableEntity
{
    public long TransportOrderItemId { get; set; }
    public long TransportOrderId { get; set; }
    public long? ProductId { get; set; }
    public long? ProductUnitId { get; set; }
    public long? UnitId { get; set; }
    public int ItemNo { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductDescription { get; set; }
    public decimal Quantity { get; set; }
    public string? UnitName { get; set; }
    public decimal? WeightKg { get; set; }
    public decimal? VolumeM3 { get; set; }
    public Product? Product { get; set; }
    public ProductUnit? ProductUnit { get; set; }
    public Unit? Unit { get; set; }
}
