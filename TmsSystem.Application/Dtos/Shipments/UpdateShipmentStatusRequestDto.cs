using System.ComponentModel.DataAnnotations;

namespace TmsSystem.Application.Dtos.Shipments;

public sealed record UpdateShipmentStatusRequestDto
{
    [Required]
    public string Status { get; init; } = string.Empty;

    public DateTime? EventDate { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }
    public string? SourceType { get; init; }
    public string? Remark { get; init; }
}
