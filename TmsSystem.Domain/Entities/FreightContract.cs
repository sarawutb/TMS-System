using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class FreightContract : AuditableEntity
{
    public long FreightContractId { get; set; }
    public long CarrierId { get; set; }
    public string ContractNo { get; set; } = string.Empty;
    public string ContractName { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string RateType { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
}
