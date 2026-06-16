using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.Application.Interfaces.Planning;

namespace TmsSystem.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/planning")]
public sealed class PlanningController(IPlanningService planningService) : TmsControllerBase
{
    [HttpGet("workbench")]
    public async Task<IActionResult> GetWorkbench(
        [FromQuery] int availableOrdersPageNumber = 1,
        [FromQuery] int routePlansPageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ApiFromOperation(await planningService.GetWorkbenchAsync(availableOrdersPageNumber, routePlansPageNumber, pageSize, cancellationToken));

    [HttpPost("routes/solve")]
    public async Task<IActionResult> SolveRoute(RoutePlanningRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await planningService.CreateRoutePlanAsync(request, cancellationToken));

    [HttpGet("routes/{routePlanId:long}")]
    public async Task<IActionResult> GetRoutePlan(
        long routePlanId,
        [FromQuery] int stopsPageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ApiFromOperation(await planningService.GetRoutePlanAsync(routePlanId, stopsPageNumber, pageSize, cancellationToken));

    [HttpPut("routes/{routePlanId:long}/stops")]
    public async Task<IActionResult> UpdateRouteStops(
        long routePlanId,
        IReadOnlyList<RouteStopDto> stops,
        [FromQuery] int stopsPageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ApiFromOperation(await planningService.UpdateRouteStopsAsync(routePlanId, stops, stopsPageNumber, pageSize, cancellationToken));

    [HttpPost("routes/{routePlanId:long}/status")]
    public async Task<IActionResult> UpdateRouteStatus(long routePlanId, PlanningApprovalRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await planningService.UpdateRouteStatusAsync(routePlanId, request.Status, cancellationToken));

    [HttpPost("loads/solve")]
    public async Task<IActionResult> SolveLoad(LoadPlanningRequestDto request, CancellationToken cancellationToken)
        => ApiFromOperation(await planningService.CreateLoadPlanAsync(request, cancellationToken));

    [HttpGet("loads/{loadPlanId:long}")]
    public async Task<IActionResult> GetLoadPlan(
        long loadPlanId,
        [FromQuery] int placementsPageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        => ApiFromOperation(await planningService.GetLoadPlanAsync(loadPlanId, placementsPageNumber, pageSize, cancellationToken));
}