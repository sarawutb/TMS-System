using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Carrier : AuditableEntity
{
    public long CarrierId { get; set; }
    public string CarrierCode { get; set; } = string.Empty;
    public string CarrierNameTh { get; set; } = string.Empty;
    public string? CarrierNameEn { get; set; }
    public string? CarrierNameShort { get; set; }
    [NotMapped]
    public string CarrierName => string.IsNullOrWhiteSpace(CarrierNameShort) ? CarrierNameTh : CarrierNameShort;
    public string? CarrierType { get; set; }
    public bool ApiEnabled { get; set; }
    public bool EdiEnabled { get; set; }
    public decimal? SafetyRating { get; set; }
    public string? TaxId { get; set; }
    public string BranchCode { get; set; } = "00000";
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
}
