using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerNameTh { get; set; } = string.Empty;
    public string? CustomerNameEn { get; set; }
    public string? CustomerNameShort { get; set; }
    [NotMapped]
    public string CustomerName => string.IsNullOrWhiteSpace(CustomerNameShort) ? CustomerNameTh : CustomerNameShort;
    public string? CustomerType { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? TaxId { get; set; }
    public string BranchCode { get; set; } = "00000";
    public string? AddressText { get; set; }
    public long? SubDistrictId { get; set; }
    public long? DistrictId { get; set; }
    public long? ProvinceId { get; set; }
    public Province? Province { get; set; }
    public District? District { get; set; }
    public SubDistrict? SubDistrict { get; set; }
}
