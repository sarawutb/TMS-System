using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/tracking-event")]
public sealed class TrackingEventsController(TmsDbContext dbContext) : CrudController<TrackingEvent>(dbContext)
{
    [HttpGet("by-driver/{driverId:long}")]
    public async Task<IActionResult> GetByDriver(long driverId, CancellationToken cancellationToken)
        => ApiSuccess(await Context.TrackingEvents
            .AsNoTracking()
            .Where(x => x.DriverId == driverId)
            .OrderByDescending(x => x.EventDate)
            .ToListAsync(cancellationToken));

    [HttpGet("by-shipment/{shipmentId:long}")]
    public async Task<IActionResult> GetByShipment(long shipmentId, CancellationToken cancellationToken)
        => ApiSuccess(await Context.TrackingEvents
            .AsNoTracking()
            .Where(x => x.ShipmentId == shipmentId)
            .OrderByDescending(x => x.EventDate)
            .ToListAsync(cancellationToken));
}
