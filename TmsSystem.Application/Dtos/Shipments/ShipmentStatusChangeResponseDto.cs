namespace TmsSystem.Application.Dtos.Shipments;

public sealed record ShipmentStatusChangeResponseDto
{
    public long ShipmentId { get; init; }
    public string ShipmentNo { get; init; } = string.Empty;
    public string PreviousStatus { get; init; } = string.Empty;
    public string CurrentStatus { get; init; } = string.Empty;
    public long? TrackingEventId { get; init; }
}
