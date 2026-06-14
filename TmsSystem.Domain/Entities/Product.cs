using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public long ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCategory { get; set; }
    public bool HazardousFlag { get; set; }
    public bool ColdChainFlag { get; set; }
    public decimal? MinTemperatureC { get; set; }
    public decimal? MaxTemperatureC { get; set; }
    public bool StackableFlag { get; set; }
}
