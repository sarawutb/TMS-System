using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class ProductGroup : AuditableEntity
{
    public long ProductGroupId { get; set; }
    public long ProductProfileId { get; set; }
    public string ProductGroupCode { get; set; } = string.Empty;
    public string ProductGroupNameTh { get; set; } = string.Empty;
    public string? ProductGroupNameEn { get; set; }
    public string? ProductGroupNameShort { get; set; }
    public string? Description { get; set; }
    public ProductProfile? ProductProfile { get; set; }
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

    [NotMapped]
    public string ProductGroupName => string.IsNullOrWhiteSpace(ProductGroupNameShort) ? ProductGroupNameTh : ProductGroupNameShort;
}
