using Microsoft.EntityFrameworkCore;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.Application.Interfaces.Planning;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Infrastructure.Services.Planning;

public sealed class HeuristicOptimizationSolverService(TmsDbContext dbContext) : IOptimizationSolverService
{
    private const decimal DefaultAverageSpeedKmPerHour = 55m;
    private const int DefaultStopServiceMinutes = 30;

    public async Task<OperationResult<RoutePlanSummaryDto>> SolveRouteAsync(RoutePlanningRequestDto request, CancellationToken cancellationToken = default)
    {
        var orderIds = request.TransportOrderIds.Distinct().ToArray();
        if (orderIds.Length == 0)
        {
            return OperationResult<RoutePlanSummaryDto>.Failure("Select at least one transport order for routing.");
        }

        var orders = await dbContext.TransportOrders
            .AsNoTracking()
            .Where(order => orderIds.Contains(order.TransportOrderId))
            .OrderBy(order => order.RequestedPickupDate ?? DateTime.MaxValue)
            .ThenBy(order => order.Priority == "High" ? 0 : order.Priority == "Medium" ? 1 : order.Priority == "Low" ? 2 : 3)
            .ToListAsync(cancellationToken);

        if (orders.Count != orderIds.Length)
        {
            return OperationResult<RoutePlanSummaryDto>.Failure("One or more selected transport orders could not be found.");
        }

        var locationIds = orders.Select(order => order.PickupLocationId)
            .Concat(orders.Select(order => order.DeliveryLocationId))
            .Distinct()
            .ToArray();
        var locations = await dbContext.Locations
            .AsNoTracking()
            .Where(location => locationIds.Contains(location.LocationId))
            .ToDictionaryAsync(location => location.LocationId, cancellationToken);

        var vehicle = request.VehicleId.HasValue
            ? await dbContext.Vehicles.AsNoTracking().Include(x => x.Carrier).FirstOrDefaultAsync(x => x.VehicleId == request.VehicleId.Value, cancellationToken)
            : await dbContext.Vehicles.AsNoTracking().Include(x => x.Carrier).OrderByDescending(x => x.CapacityWeightKg ?? 0m).FirstOrDefaultAsync(cancellationToken);
        var carrier = request.CarrierId.HasValue
            ? await dbContext.Carriers.AsNoTracking().FirstOrDefaultAsync(x => x.CarrierId == request.CarrierId.Value, cancellationToken)
            : vehicle?.Carrier;

        var issues = new List<string>();
        if (orders.Any(order => order.TemperatureRequired) && vehicle?.TemperatureControlled != true)
        {
            issues.Add("Temperature-controlled orders require a temperature-controlled vehicle.");
        }

        var stops = BuildStops(orders, locations, request.PlanDate ?? DateTime.UtcNow, issues);
        var distance = stops.Sum(stop => stop.DistanceFromPreviousKm);
        var durationMinutes = stops.Count == 0
            ? 0
            : (int)Math.Round((distance / DefaultAverageSpeedKmPerHour * 60m) + (stops.Count * DefaultStopServiceMinutes), MidpointRounding.AwayFromZero);

        foreach (var stop in stops.Where(stop => stop.StopType == "Delivery" && stop.WindowEnd.HasValue && stop.Eta.HasValue && stop.Eta > stop.WindowEnd))
        {
            issues.Add($"Order {stop.TransportOrderNo} delivery ETA is after the requested delivery window.");
        }

        var cost = CalculateCost(distance, durationMinutes, orders, vehicle, carrier);
        var riskScore = CalculateRiskScore(orders, locations, issues, distance);

        return OperationResult<RoutePlanSummaryDto>.Success(new RoutePlanSummaryDto
        {
            RoutePlanNo = string.Empty,
            PlanDate = request.PlanDate ?? DateTime.UtcNow,
            Status = "Proposed",
            OptimizationEngine = string.IsNullOrWhiteSpace(request.OptimizationEngine) ? "Heuristic" : request.OptimizationEngine.Trim(),
            TransportMode = string.IsNullOrWhiteSpace(request.TransportMode) ? "Road" : request.TransportMode.Trim(),
            VehicleType = vehicle?.VehicleType ?? "Unassigned",
            TotalDistanceKm = decimal.Round(distance, 2),
            TotalDurationMinute = durationMinutes,
            EstimatedCost = decimal.Round(cost, 2),
            RiskScore = decimal.Round(riskScore, 2),
            EarliestPickupWindow = orders.Min(order => order.RequestedPickupDate),
            LatestDeliveryWindow = orders.Max(order => order.RequestedDeliveryDate),
            Stops = stops,
            ComplianceIssues = issues
        });
    }

    public async Task<OperationResult<LoadPlanSummaryDto>> SolveLoadAsync(LoadPlanningRequestDto request, CancellationToken cancellationToken = default)
    {
        var orderIds = await ResolveLoadOrderIdsAsync(request, cancellationToken);
        if (orderIds.Count == 0)
        {
            return OperationResult<LoadPlanSummaryDto>.Failure("Select at least one transport order for load planning.");
        }

        var orders = await dbContext.TransportOrders
            .AsNoTracking()
            .Where(order => orderIds.Contains(order.TransportOrderId))
            .OrderBy(order => order.RequestedDeliveryDate ?? DateTime.MaxValue)
            .ToListAsync(cancellationToken);

        var items = await dbContext.TransportOrderItems
            .AsNoTracking()
            .Where(item => orderIds.Contains(item.TransportOrderId))
            .ToListAsync(cancellationToken);

        var vehicle = request.VehicleId.HasValue
            ? await dbContext.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.VehicleId == request.VehicleId.Value, cancellationToken)
            : await dbContext.Vehicles.AsNoTracking().OrderByDescending(x => x.CapacityWeightKg ?? 0m).FirstOrDefaultAsync(cancellationToken);

        var container = ResolveContainer(request, vehicle);
        var capacityVolume = vehicle?.CapacityVolumeM3 ?? decimal.Round(container.LengthM * container.WidthM * container.HeightM, 3);
        var capacityWeight = vehicle?.CapacityWeightKg ?? 10000m;
        var totalWeight = items.Sum(item => item.WeightKg ?? 0m);
        var totalVolume = items.Sum(item => item.VolumeM3 ?? 0m);
        var issues = new List<string>();

        if (totalWeight > capacityWeight)
        {
            issues.Add("Total cargo weight exceeds vehicle capacity.");
        }

        if (totalVolume > capacityVolume)
        {
            issues.Add("Total cargo volume exceeds vehicle capacity.");
        }

        var placements = BuildPlacements(orders, items, container, issues);
        var utilization = capacityVolume <= 0m ? 0m : Math.Min(100m, totalVolume / capacityVolume * 100m);

        return OperationResult<LoadPlanSummaryDto>.Success(new LoadPlanSummaryDto
        {
            RoutePlanId = request.RoutePlanId,
            LoadPlanNo = string.Empty,
            VehicleType = vehicle?.VehicleType ?? request.VehicleType,
            TotalWeightKg = decimal.Round(totalWeight, 2),
            TotalVolumeM3 = decimal.Round(totalVolume, 2),
            CapacityWeightKg = decimal.Round(capacityWeight, 2),
            CapacityVolumeM3 = decimal.Round(capacityVolume, 2),
            UtilizationPercent = decimal.Round(utilization, 2),
            LoadFeasible = issues.Count == 0,
            Container = container,
            Placements = placements,
            ConstraintIssues = issues
        });
    }

    private async Task<IReadOnlyList<long>> ResolveLoadOrderIdsAsync(LoadPlanningRequestDto request, CancellationToken cancellationToken)
    {
        var orderIds = request.TransportOrderIds.Distinct().ToArray();
        if (orderIds.Length > 0 || !request.RoutePlanId.HasValue)
        {
            return orderIds;
        }

        var route = await dbContext.RoutePlans.AsNoTracking().FirstOrDefaultAsync(x => x.RoutePlanId == request.RoutePlanId.Value, cancellationToken);
        if (string.IsNullOrWhiteSpace(route?.StopSequenceJson))
        {
            return Array.Empty<long>();
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<RouteStopDto>>(route.StopSequenceJson)?
                .Where(stop => stop.TransportOrderId.HasValue)
                .Select(stop => stop.TransportOrderId!.Value)
                .Distinct()
                .ToArray() ?? Array.Empty<long>();
        }
        catch (System.Text.Json.JsonException)
        {
            return Array.Empty<long>();
        }
    }

    private static IReadOnlyList<RouteStopDto> BuildStops(IReadOnlyList<TransportOrder> orders, IReadOnlyDictionary<long, Location> locations, DateTime planDate, ICollection<string> issues)
    {
        var stops = new List<RouteStopDto>();
        Location? previous = null;
        var eta = planDate;
        var sequence = 1;

        foreach (var order in orders)
        {
            AddStop(order, order.PickupLocationId, "Pickup", order.RequestedPickupDate, order.RequestedDeliveryDate);
            AddStop(order, order.DeliveryLocationId, "Delivery", order.RequestedPickupDate, order.RequestedDeliveryDate);
        }

        return stops;

        void AddStop(TransportOrder order, long locationId, string stopType, DateTime? windowStart, DateTime? windowEnd)
        {
            if (!locations.TryGetValue(locationId, out var location))
            {
                issues.Add($"Location {locationId} for order {order.TransportOrderNo} was not found.");
                return;
            }

            var legDistance = previous is null ? 0m : EstimateDistanceKm(previous, location);
            if (previous is not null && legDistance == 0m)
            {
                issues.Add($"Missing coordinates for leg into {location.LocationName}; distance was estimated as zero.");
            }

            eta = eta.AddMinutes((double)(legDistance / DefaultAverageSpeedKmPerHour * 60m));
            var cumulative = stops.Count == 0 ? legDistance : stops[^1].CumulativeDistanceKm + legDistance;
            stops.Add(new RouteStopDto
            {
                Sequence = sequence++,
                TransportOrderId = order.TransportOrderId,
                TransportOrderNo = order.TransportOrderNo,
                LocationId = location.LocationId,
                LocationName = location.LocationName,
                StopType = stopType,
                DistanceFromPreviousKm = decimal.Round(legDistance, 2),
                CumulativeDistanceKm = decimal.Round(cumulative, 2),
                Eta = eta,
                WindowStart = windowStart,
                WindowEnd = windowEnd
            });
            eta = eta.AddMinutes(DefaultStopServiceMinutes);
            previous = location;
        }
    }

    private static decimal EstimateDistanceKm(Location from, Location to)
    {
        if (!from.Latitude.HasValue || !from.Longitude.HasValue || !to.Latitude.HasValue || !to.Longitude.HasValue)
        {
            return 0m;
        }

        var fromLatitude = DegreesToRadians((double)from.Latitude.Value);
        var toLatitude = DegreesToRadians((double)to.Latitude.Value);
        var latitudeDelta = DegreesToRadians((double)(to.Latitude.Value - from.Latitude.Value));
        var longitudeDelta = DegreesToRadians((double)(to.Longitude.Value - from.Longitude.Value));
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(fromLatitude) * Math.Cos(toLatitude) * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return (decimal)(6371d * c);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180d;

    private static decimal CalculateCost(decimal distance, int durationMinutes, IReadOnlyList<TransportOrder> orders, Vehicle? vehicle, Carrier? carrier)
    {
        var vehicleMultiplier = vehicle?.VehicleType switch
        {
            "Trailer" or "Container" => 1.35m,
            "10W" => 1.2m,
            "6W" => 1.05m,
            _ => 1m
        };
        var carrierMultiplier = carrier?.SafetyRating is >= 4.5m ? 0.96m : 1m;
        var temperatureSurcharge = orders.Any(order => order.TemperatureRequired) ? 450m : 0m;
        return ((750m + (distance * 32m) + (durationMinutes / 60m * 120m)) * vehicleMultiplier * carrierMultiplier) + temperatureSurcharge;
    }

    private static decimal CalculateRiskScore(IReadOnlyList<TransportOrder> orders, IReadOnlyDictionary<long, Location> locations, ICollection<string> issues, decimal distance)
    {
        var score = 10m;
        score += orders.Count(order => string.Equals(order.Priority, "High", StringComparison.OrdinalIgnoreCase)) * 8m;
        score += orders.Count(order => order.TemperatureRequired) * 6m;
        score += issues.Count * 10m;
        score += distance > 500m ? 8m : distance > 250m ? 4m : 0m;
        score += locations.Values.Count(location => !location.Latitude.HasValue || !location.Longitude.HasValue) * 3m;
        return Math.Min(100m, score);
    }

    private static ContainerDimensionDto ResolveContainer(LoadPlanningRequestDto request, Vehicle? vehicle)
    {
        if (request.ContainerLengthM.HasValue && request.ContainerWidthM.HasValue && request.ContainerHeightM.HasValue)
        {
            return new ContainerDimensionDto
            {
                LengthM = request.ContainerLengthM.Value,
                WidthM = request.ContainerWidthM.Value,
                HeightM = request.ContainerHeightM.Value
            };
        }

        return (vehicle?.VehicleType ?? request.VehicleType) switch
        {
            "Trailer" or "Container" => new ContainerDimensionDto { LengthM = 12.0m, WidthM = 2.35m, HeightM = 2.55m },
            "10W" => new ContainerDimensionDto { LengthM = 7.2m, WidthM = 2.3m, HeightM = 2.4m },
            "6W" => new ContainerDimensionDto { LengthM = 5.5m, WidthM = 2.1m, HeightM = 2.2m },
            "Van" => new ContainerDimensionDto { LengthM = 3.2m, WidthM = 1.7m, HeightM = 1.7m },
            _ => new ContainerDimensionDto { LengthM = 4.2m, WidthM = 1.9m, HeightM = 2.0m }
        };
    }

    private static IReadOnlyList<CargoPlacementDto> BuildPlacements(IReadOnlyList<TransportOrder> orders, IReadOnlyList<TransportOrderItem> items, ContainerDimensionDto container, ICollection<string> issues)
    {
        var placements = new List<CargoPlacementDto>();
        var x = 0m;
        var y = 0m;
        var z = 0m;
        var rowDepth = 0m;

        foreach (var order in orders)
        {
            var orderItems = items.Where(item => item.TransportOrderId == order.TransportOrderId).ToArray();
            var volume = Math.Max(0.05m, orderItems.Sum(item => item.VolumeM3 ?? 0m));
            var weight = orderItems.Sum(item => item.WeightKg ?? 0m);
            var height = Math.Min(container.HeightM, Math.Max(0.3m, decimal.Round(DecimalCubeRoot(volume), 2)));
            var width = Math.Min(container.WidthM, Math.Max(0.4m, decimal.Round(DecimalCubeRoot(volume), 2)));
            var length = Math.Max(0.4m, decimal.Round(volume / Math.Max(0.01m, width * height), 2));

            if (x + length > container.LengthM)
            {
                x = 0m;
                y += rowDepth;
                rowDepth = 0m;
            }

            if (y + width > container.WidthM)
            {
                y = 0m;
                z += height;
            }

            if (z + height > container.HeightM)
            {
                issues.Add($"Order {order.TransportOrderNo} does not fit inside the current 3D load envelope.");
            }

            placements.Add(new CargoPlacementDto
            {
                TransportOrderId = order.TransportOrderId,
                TransportOrderNo = order.TransportOrderNo,
                LengthM = length,
                WidthM = width,
                HeightM = height,
                WeightKg = decimal.Round(weight, 2),
                X = decimal.Round(x, 2),
                Y = decimal.Round(y, 2),
                Z = decimal.Round(z, 2)
            });

            x += length;
            rowDepth = Math.Max(rowDepth, width);
        }

        return placements;
    }

    private static decimal DecimalCubeRoot(decimal value) => (decimal)Math.Pow((double)value, 1d / 3d);
}
