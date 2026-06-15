using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Factory : AuditableEntity
{
    public long FactoryId { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryName { get; set; } = string.Empty;
    public string IndustryType { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
