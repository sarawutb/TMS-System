using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public long ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductNameTh { get; set; } = string.Empty;
    public string? ProductNameEn { get; set; }
    public string? ProductNameShort { get; set; }
    public long? ProductCategoryId { get; set; }
    public bool HazardousFlag { get; set; }
    public bool ColdChainFlag { get; set; }
    public decimal? MinTemperatureC { get; set; }
    public decimal? MaxTemperatureC { get; set; }
    public bool StackableFlag { get; set; }
    public ProductCategory? ProductCategory { get; set; }

    [NotMapped]
    public string? ProductCategoryText { get; set; }

    [NotMapped]
    public string? LegacyProductCategory { get; set; }
    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    [NotMapped]
    public string ProductName { get => string.IsNullOrWhiteSpace(ProductNameShort) ? ProductNameTh : ProductNameShort; set => ProductNameTh = value; }
}

