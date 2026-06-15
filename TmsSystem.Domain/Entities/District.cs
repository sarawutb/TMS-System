using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class District : AuditableEntity
{
    public long DistrictId { get; set; }
    public long ProvinceId { get; set; }
    public string DistrictCode { get; set; } = string.Empty;
    public string DistrictNameTh { get; set; } = string.Empty;
    public string? DistrictNameEn { get; set; }
    public Province? Province { get; set; }
    public ICollection<SubDistrict> SubDistricts { get; set; } = new List<SubDistrict>();
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
