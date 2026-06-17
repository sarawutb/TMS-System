using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Unit : AuditableEntity
{
    public long UnitId { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitNameTh { get; set; } = string.Empty;
    public string? UnitNameEn { get; set; }
    public string? UnitNameShort { get; set; }
    public string? UnitSymbol { get; set; }
    public long? UnitProfileId { get; set; }
    public byte DecimalPrecision { get; set; } = 2;
    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    [NotMapped]
    public string UnitName => string.IsNullOrWhiteSpace(UnitNameShort) ? UnitNameTh : UnitNameShort;
}

