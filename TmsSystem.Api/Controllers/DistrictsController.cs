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
    {
        var districts = await dbContext.Districts
            .AsNoTracking()
            .Where(district => district.ProvinceId == provinceId)
            .OrderBy(district => district.DistrictNameTh)
            .ToListAsync(cancellationToken);

        return ApiSuccess(districts);
    }
}
