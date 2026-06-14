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
    {
        var connection = dbContext.Database.GetDbConnection();

        try
        {
            await connection.OpenAsync(cancellationToken);
            return ApiSuccess(new
            {
                ok = true,
                dataSource = connection.DataSource,
                database = connection.Database,
                state = connection.State.ToString()
            });
        }
        catch (Exception ex)
        {
            return ApiFailure<object>(
                StatusCodes.Status503ServiceUnavailable,
                "Database connection failed.",
                [ex.Message]);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
