namespace TmsSystem.Application.Dtos.Fleet;

public sealed record FleetSummaryDto
{
    public IReadOnlyList<VehicleFleetSummaryDto> Vehicles { get; init; } = Array.Empty<VehicleFleetSummaryDto>();
}

public sealed record VehicleFleetSummaryDto
{
    public long VehicleId { get; init; }
    public string VehicleNo { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public int ShipmentCount30Days { get; init; }
    public decimal UtilizationPercent { get; init; }
    public DateTime? NextMaintenanceDate { get; init; }
    public string MaintenanceStatus { get; init; } = string.Empty;
    public decimal? FuelEfficiencyKmPerLiter { get; init; }
    public int SafetyEventCount30Days { get; init; }
}
