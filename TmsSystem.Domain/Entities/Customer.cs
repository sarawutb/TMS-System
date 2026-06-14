using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Customer : AuditableEntity
{
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerType { get; set; }
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
}
