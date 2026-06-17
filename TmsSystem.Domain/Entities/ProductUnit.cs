using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class ProductUnit : AuditableEntity
{
    public long ProductUnitId { get; set; }
    public long ProductId { get; set; }
    public long UnitId { get; set; }
    public string? UnitRole { get; set; }
    public decimal ConversionQtyToBase { get; set; } = 1;
    public decimal? NetWeightKg { get; set; }
    public decimal? GrossWeightKg { get; set; }
    public decimal? VolumeCbm { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? HeightCm { get; set; }
    public string? Barcode { get; set; }
    public bool IsDefaultOrderUnit { get; set; }
    public bool IsDefaultTransportUnit { get; set; }
    public Product? Product { get; set; }
    public Unit? Unit { get; set; }
}
