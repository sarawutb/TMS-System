using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.Application.Interfaces.Planning;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Infrastructure.Services.Planning;

public sealed class PlanningService(TmsDbContext dbContext, IOptimizationSolverService solverService) : IPlanningService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<OperationResult<PlanningWorkbenchDto>> GetWorkbenchAsync(
        int availableOrdersPageNumber = 1,
        int routePlansPageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        availableOrdersPageNumber = Math.Max(1, availableOrdersPageNumber);
        routePlansPageNumber = Math.Max(1, routePlansPageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var ordersQuery = dbContext.TransportOrders
            .AsNoTracking()
            .Where(order => order.IsActive && order.Status != "Completed" && order.Status != "Cancelled")
            .OrderBy(order => order.RequestedPickupDate ?? DateTime.MaxValue)
            .ThenByDescending(order => order.Priority);

        var totalOrders = await ordersQuery.CountAsync(cancellationToken);
        var orders = await ordersQuery
            .Skip((availableOrdersPageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(order => order.TransportOrderId).ToArray();
        var items = await dbContext.TransportOrderItems
            .AsNoTracking()
            .Where(item => orderIds.Contains(item.TransportOrderId))
            .ToListAsync(cancellationToken);
        var locationIds = orders.Select(order => order.PickupLocationId)
            .Concat(orders.Select(order => order.DeliveryLocationId))
            .Distinct()
            .ToArray();
        var locations = await dbContext.Locations
            .AsNoTracking()
            .Where(location => locationIds.Contains(location.LocationId))
            .ToDictionaryAsync(location => location.LocationId, cancellationToken);

        var routePlansQuery = dbContext.RoutePlans
            .AsNoTracking()
            .OrderByDescending(route => route.PlanDate);
        var totalRoutePlans = await routePlansQuery.CountAsync(cancellationToken);
        var routePlans = await routePlansQuery
            .Skip((routePlansPageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var loadPlans = await dbContext.LoadPlans
            .AsNoTracking()
            .OrderByDescending(load => load.CreateDate)
            .Take(20)
            .ToListAsync(cancellationToken);

        var availableOrders = orders.Select(order => ToPlanningOrder(order, items, locations)).ToArray();
        var routeSummaries = routePlans.Select(ToRouteSummary).ToArray();

        return OperationResult<PlanningWorkbenchDto>.Success(new PlanningWorkbenchDto
        {
            AvailableOrders = availableOrders,
            RoutePlans = routeSummaries,
            LoadPlans = loadPlans.Select(ToLoadSummary).ToArray(),
            AvailableOrdersPage = PagedResult<PlanningOrderDto>.Create(availableOrders, totalOrders, availableOrdersPageNumber, pageSize),
            RoutePlansPage = PagedResult<RoutePlanSummaryDto>.Create(routeSummaries, totalRoutePlans, routePlansPageNumber, pageSize)
        });
    }

    public async Task<OperationResult<RoutePlanSummaryDto>> CreateRoutePlanAsync(RoutePlanningRequestDto request, CancellationToken cancellationToken = default)
    {
        var solved = await solverService.SolveRouteAsync(request, cancellationToken);
        if (!solved.IsSuccess || solved.Data is null || !request.PersistPlan)
        {
            return solved;
        }

        var route = new RoutePlan
        {
            RoutePlanNo = await NextRoutePlanNoAsync(cancellationToken),
            PlanDate = solved.Data.PlanDate,
            OptimizationEngine = solved.Data.OptimizationEngine,
            TransportMode = solved.Data.TransportMode,
            VehicleType = solved.Data.VehicleType,
            VehicleId = request.VehicleId,
            CarrierId = request.CarrierId,
            TotalDistanceKm = solved.Data.TotalDistanceKm,
            TotalDurationMinute = solved.Data.TotalDurationMinute,
            EstimatedCost = solved.Data.EstimatedCost,
            RiskScore = solved.Data.RiskScore,
            EarliestPickupWindow = solved.Data.EarliestPickupWindow,
            LatestDeliveryWindow = solved.Data.LatestDeliveryWindow,
            StopSequenceJson = JsonSerializer.Serialize(solved.Data.Stops, JsonOptions),
            ComplianceIssuesJson = JsonSerializer.Serialize(solved.Data.ComplianceIssues, JsonOptions),
            SolverMetadataJson = JsonSerializer.Serialize(new
            {
                request.TransportOrderIds,
                request.OptimizationEngine,
                generatedAt = DateTime.UtcNow
            }, JsonOptions),
            Status = solved.Data.Status,
            IsActive = true,
            CreateBy = "planning-service",
            CreateDate = DateTime.UtcNow
        };

        dbContext.RoutePlans.Add(route);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<RoutePlanSummaryDto>.Success(ToRouteSummary(route), "Route plan created.");
    }

    public async Task<OperationResult<LoadPlanSummaryDto>> CreateLoadPlanAsync(LoadPlanningRequestDto request, CancellationToken cancellationToken = default)
    {
        var solved = await solverService.SolveLoadAsync(request, cancellationToken);
        if (!solved.IsSuccess || solved.Data is null || !request.PersistPlan)
        {
            return solved;
        }

        var loadPlanNo = await NextLoadPlanNoAsync(cancellationToken);
        var load = new LoadPlan
        {
            RoutePlanId = request.RoutePlanId,
            LoadPlanNo = loadPlanNo,
            VehicleType = solved.Data.VehicleType,
            VehicleId = request.VehicleId,
            ContainerLengthM = solved.Data.Container.LengthM,
            ContainerWidthM = solved.Data.Container.WidthM,
            ContainerHeightM = solved.Data.Container.HeightM,
            CapacityWeightKg = solved.Data.CapacityWeightKg,
            CapacityVolumeM3 = solved.Data.CapacityVolumeM3,
            TotalWeightKg = solved.Data.TotalWeightKg,
            TotalVolumeM3 = solved.Data.TotalVolumeM3,
            UtilizationPercent = solved.Data.UtilizationPercent,
            ThreeDPlanRef = $"load-plan:{loadPlanNo}",
            LoadPlanJson = JsonSerializer.Serialize(solved.Data, JsonOptions),
            PlacementJson = JsonSerializer.Serialize(solved.Data.Placements, JsonOptions),
            ConstraintIssuesJson = JsonSerializer.Serialize(solved.Data.ConstraintIssues, JsonOptions),
            LoadFeasible = solved.Data.LoadFeasible,
            IsActive = true,
            CreateBy = "planning-service",
            CreateDate = DateTime.UtcNow
        };

        dbContext.LoadPlans.Add(load);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<LoadPlanSummaryDto>.Success(ToLoadSummary(load), "Load plan created.");
    }

    public async Task<OperationResult<RoutePlanSummaryDto>> GetRoutePlanAsync(long routePlanId, int stopsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var route = await dbContext.RoutePlans.AsNoTracking().FirstOrDefaultAsync(x => x.RoutePlanId == routePlanId, cancellationToken);
        return route is null
            ? OperationResult<RoutePlanSummaryDto>.Failure("Route plan not found.")
            : OperationResult<RoutePlanSummaryDto>.Success(ToRouteSummary(route, stopsPageNumber, pageSize));
    }

    public async Task<OperationResult<LoadPlanSummaryDto>> GetLoadPlanAsync(long loadPlanId, int placementsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var load = await dbContext.LoadPlans.AsNoTracking().FirstOrDefaultAsync(x => x.LoadPlanId == loadPlanId, cancellationToken);
        return load is null
            ? OperationResult<LoadPlanSummaryDto>.Failure("Load plan not found.")
            : OperationResult<LoadPlanSummaryDto>.Success(ToLoadSummary(load, placementsPageNumber, pageSize));
    }

    public async Task<OperationResult<RoutePlanSummaryDto>> UpdateRouteStopsAsync(long routePlanId, IReadOnlyList<RouteStopDto> stops, int stopsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var route = await dbContext.RoutePlans.FirstOrDefaultAsync(x => x.RoutePlanId == routePlanId, cancellationToken);
        if (route is null)
        {
            return OperationResult<RoutePlanSummaryDto>.Failure("Route plan not found.");
        }

        var resequenced = stops.Select((stop, index) => stop with { Sequence = index + 1 }).ToArray();
        route.StopSequenceJson = JsonSerializer.Serialize(resequenced, JsonOptions);
        route.TotalDistanceKm = resequenced.LastOrDefault()?.CumulativeDistanceKm ?? 0m;
        route.TotalDurationMinute = CalculateDurationFromStops(resequenced);
        route.Status = "ManualAdjusted";
        route.ReviseBy = "planning-service";
        route.ReviseDate = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<RoutePlanSummaryDto>.Success(ToRouteSummary(route, stopsPageNumber, pageSize), "Route stops updated.");
    }

    public async Task<OperationResult<RoutePlanSummaryDto>> UpdateRouteStatusAsync(long routePlanId, string status, CancellationToken cancellationToken = default)
    {
        var route = await dbContext.RoutePlans.FirstOrDefaultAsync(x => x.RoutePlanId == routePlanId, cancellationToken);
        if (route is null)
        {
            return OperationResult<RoutePlanSummaryDto>.Failure("Route plan not found.");
        }

        route.Status = string.IsNullOrWhiteSpace(status) ? "Approved" : status.Trim();
        route.ReviseBy = "planning-service";
        route.ReviseDate = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<RoutePlanSummaryDto>.Success(ToRouteSummary(route), "Route status updated.");
    }

    private static PlanningOrderDto ToPlanningOrder(TransportOrder order, IReadOnlyList<TransportOrderItem> items, IReadOnlyDictionary<long, Location> locations)
    {
        var orderItems = items.Where(item => item.TransportOrderId == order.TransportOrderId).ToArray();
        locations.TryGetValue(order.PickupLocationId, out var pickup);
        locations.TryGetValue(order.DeliveryLocationId, out var delivery);

        return new PlanningOrderDto
        {
            TransportOrderId = order.TransportOrderId,
            TransportOrderNo = order.TransportOrderNo,
            Priority = order.Priority,
            Status = order.Status,
            TemperatureRequired = order.TemperatureRequired,
            RequestedPickupDate = order.RequestedPickupDate,
            RequestedDeliveryDate = order.RequestedDeliveryDate,
            PickupLocationId = order.PickupLocationId,
            DeliveryLocationId = order.DeliveryLocationId,
            PickupLocationName = pickup?.LocationName ?? $"Location {order.PickupLocationId}",
            DeliveryLocationName = delivery?.LocationName ?? $"Location {order.DeliveryLocationId}",
            TotalWeightKg = orderItems.Sum(item => item.WeightKg ?? 0m),
            TotalVolumeM3 = orderItems.Sum(item => item.VolumeM3 ?? 0m)
        };
    }

    private static RoutePlanSummaryDto ToRouteSummary(RoutePlan route)
        => ToRouteSummary(route, 1, 10);

    private static RoutePlanSummaryDto ToRouteSummary(RoutePlan route, int stopsPageNumber, int pageSize)
    {
        var stops = Deserialize<IReadOnlyList<RouteStopDto>>(route.StopSequenceJson)?.ToArray() ?? Array.Empty<RouteStopDto>();
        stopsPageNumber = Math.Max(1, stopsPageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return new RoutePlanSummaryDto
        {
            RoutePlanId = route.RoutePlanId,
            RoutePlanNo = route.RoutePlanNo,
            PlanDate = route.PlanDate,
            Status = route.Status,
            OptimizationEngine = route.OptimizationEngine ?? string.Empty,
            TransportMode = route.TransportMode ?? string.Empty,
            VehicleType = route.VehicleType ?? string.Empty,
            TotalDistanceKm = route.TotalDistanceKm ?? 0m,
            TotalDurationMinute = route.TotalDurationMinute ?? 0,
            EstimatedCost = route.EstimatedCost ?? 0m,
            RiskScore = route.RiskScore ?? 0m,
            EarliestPickupWindow = route.EarliestPickupWindow,
            LatestDeliveryWindow = route.LatestDeliveryWindow,
            Stops = stops,
            StopsPage = PagedResult<RouteStopDto>.Create(stops, stopsPageNumber, pageSize),
            ComplianceIssues = Deserialize<IReadOnlyList<string>>(route.ComplianceIssuesJson) ?? Array.Empty<string>()
        };
    }

    private static LoadPlanSummaryDto ToLoadSummary(LoadPlan load)
        => ToLoadSummary(load, 1, 10);

    private static LoadPlanSummaryDto ToLoadSummary(LoadPlan load, int placementsPageNumber, int pageSize)
    {
        var placements = Deserialize<IReadOnlyList<CargoPlacementDto>>(load.PlacementJson)?.ToArray() ?? Array.Empty<CargoPlacementDto>();
        placementsPageNumber = Math.Max(1, placementsPageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return new LoadPlanSummaryDto
        {
            LoadPlanId = load.LoadPlanId,
            RoutePlanId = load.RoutePlanId,
            LoadPlanNo = load.LoadPlanNo,
            VehicleType = load.VehicleType,
            TotalWeightKg = load.TotalWeightKg ?? 0m,
            TotalVolumeM3 = load.TotalVolumeM3 ?? 0m,
            CapacityWeightKg = load.CapacityWeightKg ?? 0m,
            CapacityVolumeM3 = load.CapacityVolumeM3 ?? 0m,
            UtilizationPercent = load.UtilizationPercent ?? 0m,
            LoadFeasible = load.LoadFeasible,
            Container = new ContainerDimensionDto
            {
                LengthM = load.ContainerLengthM ?? 0m,
                WidthM = load.ContainerWidthM ?? 0m,
                HeightM = load.ContainerHeightM ?? 0m
            },
            Placements = placements,
            PlacementsPage = PagedResult<CargoPlacementDto>.Create(placements, placementsPageNumber, pageSize),
            ConstraintIssues = Deserialize<IReadOnlyList<string>>(load.ConstraintIssuesJson) ?? Array.Empty<string>()
        };
    }

    private static T? Deserialize<T>(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static int CalculateDurationFromStops(IReadOnlyList<RouteStopDto> stops)
    {
        var firstEta = stops.FirstOrDefault(stop => stop.Eta.HasValue)?.Eta;
        var lastEta = stops.LastOrDefault(stop => stop.Eta.HasValue)?.Eta;
        return firstEta.HasValue && lastEta.HasValue
            ? Math.Max(0, (int)Math.Round((lastEta.Value - firstEta.Value).TotalMinutes, MidpointRounding.AwayFromZero))
            : 0;
    }

    private async Task<string> NextRoutePlanNoAsync(CancellationToken cancellationToken)
    {
        var prefix = $"RP{DateTime.UtcNow:yyyyMMdd}";
        var count = await dbContext.RoutePlans.CountAsync(route => route.RoutePlanNo.StartsWith(prefix), cancellationToken);
        return $"{prefix}-{count + 1:000}";
    }

    private async Task<string> NextLoadPlanNoAsync(CancellationToken cancellationToken)
    {
        var prefix = $"LP{DateTime.UtcNow:yyyyMMdd}";
        var count = await dbContext.LoadPlans.CountAsync(load => load.LoadPlanNo.StartsWith(prefix), cancellationToken);
        return $"{prefix}-{count + 1:000}";
    }
}