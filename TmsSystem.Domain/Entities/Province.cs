using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Province : AuditableEntity
{
    public long ProvinceId { get; set; }
    public string ProvinceCode { get; set; } = string.Empty;
    public string ProvinceNameTh { get; set; } = string.Empty;
    public string? ProvinceNameEn { get; set; }
    public ICollection<District> Districts { get; set; } = new List<District>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
