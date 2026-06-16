using TmsSystem.Application.Common;

namespace TmsSystem.Application.Dtos.Planning;

public sealed record PlanningWorkbenchDto
{
    public IReadOnlyList<PlanningOrderDto> AvailableOrders { get; init; } = Array.Empty<PlanningOrderDto>();
    public IReadOnlyList<RoutePlanSummaryDto> RoutePlans { get; init; } = Array.Empty<RoutePlanSummaryDto>();
    public IReadOnlyList<LoadPlanSummaryDto> LoadPlans { get; init; } = Array.Empty<LoadPlanSummaryDto>();
    public PagedResult<PlanningOrderDto> AvailableOrdersPage { get; init; } = PagedResult<PlanningOrderDto>.Empty();
    public PagedResult<RoutePlanSummaryDto> RoutePlansPage { get; init; } = PagedResult<RoutePlanSummaryDto>.Empty();
}

public sealed record PlanningOrderDto
{
    public long TransportOrderId { get; init; }
    public string TransportOrderNo { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool TemperatureRequired { get; init; }
    public DateTime? RequestedPickupDate { get; init; }
    public DateTime? RequestedDeliveryDate { get; init; }
    public long PickupLocationId { get; init; }
    public long DeliveryLocationId { get; init; }
    public string PickupLocationName { get; init; } = string.Empty;
    public string DeliveryLocationName { get; init; } = string.Empty;
    public decimal TotalWeightKg { get; init; }
    public decimal TotalVolumeM3 { get; init; }
};

public sealed record RoutePlanningRequestDto
{
    public IReadOnlyList<long> TransportOrderIds { get; init; } = Array.Empty<long>();
    public long? VehicleId { get; init; }
    public long? CarrierId { get; init; }
    public string TransportMode { get; init; } = "Road";
    public string OptimizationEngine { get; init; } = "Heuristic";
    public DateTime? PlanDate { get; init; }
    public bool PersistPlan { get; init; } = true;
};

public sealed record RoutePlanSummaryDto
{
    public long RoutePlanId { get; init; }
    public string RoutePlanNo { get; init; } = string.Empty;
    public DateTime PlanDate { get; init; }
    public string Status { get; init; } = string.Empty;
    public string OptimizationEngine { get; init; } = string.Empty;
    public string TransportMode { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public decimal TotalDistanceKm { get; init; }
    public int TotalDurationMinute { get; init; }
    public decimal EstimatedCost { get; init; }
    public decimal RiskScore { get; init; }
    public DateTime? EarliestPickupWindow { get; init; }
    public DateTime? LatestDeliveryWindow { get; init; }
    public IReadOnlyList<RouteStopDto> Stops { get; init; } = Array.Empty<RouteStopDto>();
    public PagedResult<RouteStopDto> StopsPage { get; init; } = PagedResult<RouteStopDto>.Empty();
    public IReadOnlyList<string> ComplianceIssues { get; init; } = Array.Empty<string>();
};

public sealed record RouteStopDto
{
    public int Sequence { get; init; }
    public long? TransportOrderId { get; init; }
    public string TransportOrderNo { get; init; } = string.Empty;
    public long LocationId { get; init; }
    public string LocationName { get; init; } = string.Empty;
    public string StopType { get; init; } = string.Empty;
    public decimal DistanceFromPreviousKm { get; init; }
    public decimal CumulativeDistanceKm { get; init; }
    public DateTime? Eta { get; init; }
    public DateTime? WindowStart { get; init; }
    public DateTime? WindowEnd { get; init; }
};

public sealed record LoadPlanningRequestDto
{
    public long? RoutePlanId { get; init; }
    public IReadOnlyList<long> TransportOrderIds { get; init; } = Array.Empty<long>();
    public long? VehicleId { get; init; }
    public string VehicleType { get; init; } = "Truck";
    public decimal? ContainerLengthM { get; init; }
    public decimal? ContainerWidthM { get; init; }
    public decimal? ContainerHeightM { get; init; }
    public bool PersistPlan { get; init; } = true;
};

public sealed record LoadPlanSummaryDto
{
    public long LoadPlanId { get; init; }
    public long? RoutePlanId { get; init; }
    public string LoadPlanNo { get; init; } = string.Empty;
    public string VehicleType { get; init; } = string.Empty;
    public decimal TotalWeightKg { get; init; }
    public decimal TotalVolumeM3 { get; init; }
    public decimal CapacityWeightKg { get; init; }
    public decimal CapacityVolumeM3 { get; init; }
    public decimal UtilizationPercent { get; init; }
    public bool LoadFeasible { get; init; }
    public ContainerDimensionDto Container { get; init; } = new();
    public IReadOnlyList<CargoPlacementDto> Placements { get; init; } = Array.Empty<CargoPlacementDto>();
    public PagedResult<CargoPlacementDto> PlacementsPage { get; init; } = PagedResult<CargoPlacementDto>.Empty();
    public IReadOnlyList<string> ConstraintIssues { get; init; } = Array.Empty<string>();
};

public sealed record ContainerDimensionDto
{
    public decimal LengthM { get; init; }
    public decimal WidthM { get; init; }
    public decimal HeightM { get; init; }
};

public sealed record CargoPlacementDto
{
    public long TransportOrderId { get; init; }
    public string TransportOrderNo { get; init; } = string.Empty;
    public decimal LengthM { get; init; }
    public decimal WidthM { get; init; }
    public decimal HeightM { get; init; }
    public decimal WeightKg { get; init; }
    public decimal X { get; init; }
    public decimal Y { get; init; }
    public decimal Z { get; init; }
};

public sealed record PlanningApprovalRequestDto
{
    public string Status { get; init; } = "Approved";
};