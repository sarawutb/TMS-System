using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class ProductCategory : AuditableEntity
{
    public long ProductCategoryId { get; set; }
    public long ProductGroupId { get; set; }
    public string ProductCategoryCode { get; set; } = string.Empty;
    public string ProductCategoryNameTh { get; set; } = string.Empty;
    public string? ProductCategoryNameEn { get; set; }
    public string? ProductCategoryNameShort { get; set; }
    public string? Description { get; set; }
    public ProductGroup? ProductGroup { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();

    [NotMapped]
    public string ProductCategoryName => string.IsNullOrWhiteSpace(ProductCategoryNameShort) ? ProductCategoryNameTh : ProductCategoryNameShort;
}
