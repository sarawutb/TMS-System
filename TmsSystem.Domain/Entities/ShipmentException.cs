using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class ShipmentException : AuditableEntity
{
    public long ExceptionId { get; set; }
    public long ShipmentId { get; set; }
    public long ExceptionProfileId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime ExceptionDate { get; set; }
    public string? Description { get; set; }
    public string ResolutionStatus { get; set; } = string.Empty;
    public string? ResolutionNote { get; set; }

    [NotMapped]
    public string ExceptionType { get; set; } = string.Empty;
}

