using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class IntegrationPartner : AuditableEntity
{
    public long IntegrationPartnerId { get; set; }
    public string PartnerCode { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string SystemType { get; set; } = string.Empty;
    public string IntegrationMethod { get; set; } = string.Empty;
    public string? EndpointUrl { get; set; }
}
