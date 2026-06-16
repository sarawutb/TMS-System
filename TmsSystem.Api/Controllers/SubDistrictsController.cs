using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/subdistrict")]
public sealed class SubDistrictsController(TmsDbContext dbContext) : CrudController<SubDistrict>(dbContext)
{
    [HttpGet("by-district/{districtId:long}")]
    public async Task<IActionResult> GetByDistrict(long districtId, CancellationToken cancellationToken)
        => ApiSuccess(await Context.SubDistricts
            .AsNoTracking()
            .Where(sd => sd.DistrictId == districtId)
            .OrderBy(sd => sd.SubDistrictNameTh)
            .ToListAsync(cancellationToken));
}
