using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/district")]
public sealed class DistrictsController(TmsDbContext dbContext) : CrudController<District>(dbContext)
{
    [HttpGet("by-province/{provinceId:long}")]
    public async Task<IActionResult> GetByProvince(long provinceId, CancellationToken cancellationToken)
        => ApiSuccess(await Context.Districts
            .AsNoTracking()
            .Where(d => d.ProvinceId == provinceId)
            .OrderBy(d => d.DistrictNameTh)
            .ToListAsync(cancellationToken));
}
