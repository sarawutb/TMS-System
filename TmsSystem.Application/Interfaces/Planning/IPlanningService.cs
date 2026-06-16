using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Planning;

namespace TmsSystem.Application.Interfaces.Planning;

public interface IPlanningService
{
    Task<OperationResult<PlanningWorkbenchDto>> GetWorkbenchAsync(
        int availableOrdersPageNumber = 1,
        int routePlansPageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
    Task<OperationResult<RoutePlanSummaryDto>> CreateRoutePlanAsync(RoutePlanningRequestDto request, CancellationToken cancellationToken = default);
    Task<OperationResult<LoadPlanSummaryDto>> CreateLoadPlanAsync(LoadPlanningRequestDto request, CancellationToken cancellationToken = default);
    Task<OperationResult<RoutePlanSummaryDto>> GetRoutePlanAsync(long routePlanId, int stopsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<OperationResult<LoadPlanSummaryDto>> GetLoadPlanAsync(long loadPlanId, int placementsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<OperationResult<RoutePlanSummaryDto>> UpdateRouteStopsAsync(long routePlanId, IReadOnlyList<RouteStopDto> stops, int stopsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<OperationResult<RoutePlanSummaryDto>> UpdateRouteStatusAsync(long routePlanId, string status, CancellationToken cancellationToken = default);
}