using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Planning;

namespace TmsSystem.Application.Interfaces.Planning;

public interface IOptimizationSolverService
{
    Task<OperationResult<RoutePlanSummaryDto>> SolveRouteAsync(RoutePlanningRequestDto request, CancellationToken cancellationToken = default);
    Task<OperationResult<LoadPlanSummaryDto>> SolveLoadAsync(LoadPlanningRequestDto request, CancellationToken cancellationToken = default);
}
