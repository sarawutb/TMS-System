using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Factory : AuditableEntity
{
    public long FactoryId { get; set; }
    public string FactoryCode { get; set; } = string.Empty;
    public string FactoryNameTh { get; set; } = string.Empty;
    public string? FactoryNameEn { get; set; }
    public string? FactoryNameShort { get; set; }
    public long IndustryProfileId { get; set; }
    public string? TimeZone { get; set; }
    public string? TaxId { get; set; }
    public string? BranchCode { get; set; } = "00000";
    public ICollection<Location> Locations { get; set; } = new List<Location>();

    [NotMapped]
    public string FactoryName => string.IsNullOrWhiteSpace(FactoryNameShort) ? FactoryNameTh : FactoryNameShort;

    [NotMapped]
    public string IndustryType { get; set; } = string.Empty;
}

