using System.ComponentModel.DataAnnotations.Schema;
using TmsSystem.Domain.Common;

namespace TmsSystem.Domain.Entities;

public sealed class Profile : AuditableEntity
{
    public long ProfileId { get; set; }
    public string ProfileScope { get; set; } = string.Empty;
    public string ProfileCode { get; set; } = string.Empty;
    public string ProfileNameTh { get; set; } = string.Empty;
    public string? ProfileNameEn { get; set; }
    public string? ProfileNameShort { get; set; }
    public string? Description { get; set; }

    [NotMapped]
    public string ProfileName => string.IsNullOrWhiteSpace(ProfileNameShort) ? ProfileNameTh : ProfileNameShort;
}
