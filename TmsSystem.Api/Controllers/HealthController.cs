using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/health")]
public sealed class HealthController(TmsDbContext dbContext) : TmsControllerBase
{
    [HttpGet("database")]
    public async Task<IActionResult> Database(CancellationToken cancellationToken)
        // ponytail: use native EF Core CanConnectAsync, avoiding manual connection opening/closing boilerplates
        => await dbContext.Database.CanConnectAsync(cancellationToken)
            ? ApiSuccess(new { ok = true, database = dbContext.Database.GetDbConnection().Database })
            : ApiFailure<object>(StatusCodes.Status503ServiceUnavailable, "Database connection failed.");
}
