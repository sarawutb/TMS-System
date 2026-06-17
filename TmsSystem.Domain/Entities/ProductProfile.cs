using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class ProductProfile : AuditableEntity
{
    public long ProductProfileId { get; set; }
    public string ProductProfileCode { get; set; } = string.Empty;
    public string ProductProfileNameTh { get; set; } = string.Empty;
    public string? ProductProfileNameEn { get; set; }
    public string? ProductProfileNameShort { get; set; }
    public string? Description { get; set; }
    public ICollection<ProductGroup> ProductGroups { get; set; } = new List<ProductGroup>();

    [NotMapped]
    public string ProductProfileName => string.IsNullOrWhiteSpace(ProductProfileNameShort) ? ProductProfileNameTh : ProductProfileNameShort;
}
