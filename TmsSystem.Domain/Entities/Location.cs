using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Location : AuditableEntity
{
    public long LocationId { get; set; }
    public long? FactoryId { get; set; }
    public string LocationCode { get; set; } = string.Empty;
    public string LocationNameTh { get; set; } = string.Empty;
    public string? LocationNameEn { get; set; }
    public string? LocationNameShort { get; set; }
    [NotMapped]
    public string LocationName => string.IsNullOrWhiteSpace(LocationNameShort) ? LocationNameTh : LocationNameShort;
    public string LocationType { get; set; } = string.Empty;
    public string? AddressText { get; set; }
    public long? SubDistrictId { get; set; }
    public long? DistrictId { get; set; }
    public long? ProvinceId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? DockCode { get; set; }
    public Factory? Factory { get; set; }
    public Province? Province { get; set; }
    public District? District { get; set; }
    public SubDistrict? SubDistrict { get; set; }
}
