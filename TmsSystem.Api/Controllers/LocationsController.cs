using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/location")]
public sealed class LocationsController(TmsDbContext dbContext) : CrudController<Location>(dbContext)
{
    [HttpGet("by-factory/{factoryId:long}")]
    public async Task<IActionResult> GetByFactory(long factoryId, CancellationToken cancellationToken)
    {
        var locations = await Context.Locations
            .AsNoTracking()
            .Where(location => location.FactoryId == factoryId)
            .OrderBy(location => location.LocationName)
            .ToListAsync(cancellationToken);

        return ApiSuccess(locations);
    }
}
