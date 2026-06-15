using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class SubDistrict : AuditableEntity
{
    public long SubDistrictId { get; set; }
    public long DistrictId { get; set; }
    public string SubDistrictCode { get; set; } = string.Empty;
    public string SubDistrictNameTh { get; set; } = string.Empty;
    public string? SubDistrictNameEn { get; set; }
    public string? PostalCode { get; set; }
    public District? District { get; set; }
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
