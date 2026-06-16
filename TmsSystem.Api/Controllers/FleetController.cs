using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Application.Dtos.Fleet;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/fleet")]
public sealed class FleetController(TmsDbContext dbContext) : TmsControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var since = today.AddDays(-30);

        var vehicles = await dbContext.Vehicles.AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken);
        var vehicleIds = vehicles.Select(x => x.VehicleId).ToArray();
        var shipments = await dbContext.Shipments.AsNoTracking().Where(x => x.VehicleId.HasValue && vehicleIds.Contains(x.VehicleId.Value) && (x.PlannedPickupDate ?? x.ActualPickupDate ?? x.CreateDate) >= since).ToListAsync(cancellationToken);
        var maintenance = await dbContext.VehicleMaintenances.AsNoTracking().Where(x => vehicleIds.Contains(x.VehicleId) && x.CompleteDate == null && x.MaintenanceStatus != "Completed" && x.MaintenanceStatus != "Cancelled").ToListAsync(cancellationToken);
        var fuel = await dbContext.FuelTransactions.AsNoTracking().Where(x => vehicleIds.Contains(x.VehicleId) && x.FuelDate >= since).ToListAsync(cancellationToken);
        var safety = await dbContext.TrackingEvents.AsNoTracking().Where(x => x.VehicleId.HasValue && vehicleIds.Contains(x.VehicleId.Value) && x.EventDate >= since && (x.SafetyEventType != null || x.SourceType == "Telematics" || x.SourceType == "MDVR")).ToListAsync(cancellationToken);

        var summary = vehicles.Select(vehicle =>
        {
            var vehicleShipments = shipments.Where(x => x.VehicleId == vehicle.VehicleId).ToArray();
            var activeDays = vehicleShipments
                .Select(x => (x.PlannedPickupDate ?? x.ActualPickupDate ?? x.CreateDate).Date)
                .Distinct()
                .Count();
            var nextMaintenance = maintenance
                .Where(x => x.VehicleId == vehicle.VehicleId)
                .OrderBy(x => x.ScheduleDate)
                .FirstOrDefault();
            var fuelEfficiency = CalculateFuelEfficiency(fuel.Where(x => x.VehicleId == vehicle.VehicleId));

            return new VehicleFleetSummaryDto
            {
                VehicleId = vehicle.VehicleId,
                VehicleNo = vehicle.VehicleNo,
                VehicleType = vehicle.VehicleType,
                ShipmentCount30Days = vehicleShipments.Length,
                UtilizationPercent = decimal.Round(activeDays / 30m * 100m, 2),
                NextMaintenanceDate = nextMaintenance?.ScheduleDate,
                MaintenanceStatus = nextMaintenance is null ? "No Open Maintenance" : nextMaintenance.ScheduleDate.Date < today ? "Overdue" : nextMaintenance.MaintenanceStatus,
                FuelEfficiencyKmPerLiter = fuelEfficiency,
                SafetyEventCount30Days = safety.Count(x => x.VehicleId == vehicle.VehicleId)
            };
        }).OrderByDescending(x => x.UtilizationPercent).ThenBy(x => x.VehicleNo).ToArray();

        return ApiSuccess(new FleetSummaryDto { Vehicles = summary });
    }

    private static decimal? CalculateFuelEfficiency(IEnumerable<TmsSystem.Domain.Entities.FuelTransaction> rows)
    {
        var ordered = rows.Where(x => x.OdometerKm.HasValue).OrderBy(x => x.FuelDate).ToArray();
        if (ordered.Length < 2)
        {
            return null;
        }

        var km = ordered[^1].OdometerKm!.Value - ordered[0].OdometerKm!.Value;
        var liters = ordered.Skip(1).Sum(x => x.FuelLiter);
        return km > 0m && liters > 0m ? decimal.Round(km / liters, 2) : null;
    }
}
