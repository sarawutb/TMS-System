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
        => ApiSuccess(await Context.Locations
            .AsNoTracking()
            .Where(l => l.FactoryId == factoryId)
            .OrderBy(l => l.LocationNameTh)
            .ToListAsync(cancellationToken));
}
