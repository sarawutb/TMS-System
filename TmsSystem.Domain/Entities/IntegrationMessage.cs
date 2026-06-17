using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class IntegrationMessage : AuditableEntity
{
    public long IntegrationMessageId { get; set; }
    public long IntegrationPartnerId { get; set; }
    public string Direction { get; set; } = string.Empty;
    public long MessageProfileId { get; set; }
    public string? ReferenceNo { get; set; }
    public string? PayloadRef { get; set; }
    public string MessageStatus { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessDate { get; set; }

    [NotMapped]
    public string MessageType { get; set; } = string.Empty;
}

