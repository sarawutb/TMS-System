using System.Net.Http.Json;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.BlazorWasm.Models;

namespace TmsSystem.BlazorWasm.Services;

public sealed class PlanningService(HttpClient httpClient)
{
    public Task<OperationResult<PlanningWorkbenchDto>> GetWorkbenchAsync(
        int availableOrdersPageNumber = 1,
        int routePlansPageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString([
            ("availableOrdersPageNumber", availableOrdersPageNumber.ToString()),
            ("routePlansPageNumber", routePlansPageNumber.ToString()),
            ("pageSize", pageSize.ToString())
        ]);
        return GetAsync<PlanningWorkbenchDto>($"api/planning/workbench{query}", "Failed to retrieve planning workbench.", cancellationToken);
    }

    public Task<OperationResult<RoutePlanSummaryDto>> GetRoutePlanAsync(long routePlanId, int stopsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString([
            ("stopsPageNumber", stopsPageNumber.ToString()),
            ("pageSize", pageSize.ToString())
        ]);
        return GetAsync<RoutePlanSummaryDto>($"api/planning/routes/{routePlanId}{query}", "Failed to retrieve route plan.", cancellationToken);
    }

    public Task<OperationResult<LoadPlanSummaryDto>> GetLoadPlanAsync(long loadPlanId, int placementsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString([
            ("placementsPageNumber", placementsPageNumber.ToString()),
            ("pageSize", pageSize.ToString())
        ]);
        return GetAsync<LoadPlanSummaryDto>($"api/planning/loads/{loadPlanId}{query}", "Failed to retrieve load plan.", cancellationToken);
    }

    public Task<OperationResult<RoutePlanSummaryDto>> SolveRouteAsync(RoutePlanningRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<RoutePlanningRequestDto, RoutePlanSummaryDto>("api/planning/routes/solve", request, "Failed to create route plan.", cancellationToken);

    public Task<OperationResult<LoadPlanSummaryDto>> SolveLoadAsync(LoadPlanningRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<LoadPlanningRequestDto, LoadPlanSummaryDto>("api/planning/loads/solve", request, "Failed to create load plan.", cancellationToken);

    public Task<OperationResult<RoutePlanSummaryDto>> UpdateRouteStopsAsync(long routePlanId, IReadOnlyList<RouteStopDto> stops, int stopsPageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString([
            ("stopsPageNumber", stopsPageNumber.ToString()),
            ("pageSize", pageSize.ToString())
        ]);
        return PutAsync<IReadOnlyList<RouteStopDto>, RoutePlanSummaryDto>($"api/planning/routes/{routePlanId}/stops{query}", stops, "Failed to update route stops.", cancellationToken);
    }

    public Task<OperationResult<RoutePlanSummaryDto>> ApproveRouteAsync(long routePlanId, CancellationToken cancellationToken = default)
        => PostAsync<PlanningApprovalRequestDto, RoutePlanSummaryDto>(
            $"api/planning/routes/{routePlanId}/status",
            new PlanningApprovalRequestDto(),
            "Failed to approve route plan.",
            cancellationToken);

    private async Task<OperationResult<T>> GetAsync<T>(string uri, string failureMessage, CancellationToken cancellationToken)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<T>>(uri, cancellationToken);
            return ToOperationResult(result, failureMessage);
        }
        catch (Exception ex)
        {
            return OperationResult<T>.Failure($"Connection error: {ex.Message}");
        }
    }

    private async Task<OperationResult<TResponse>> PostAsync<TRequest, TResponse>(string uri, TRequest request, string failureMessage, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(uri, request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: cancellationToken);
            return ToOperationResult(result, failureMessage);
        }
        catch (Exception ex)
        {
            return OperationResult<TResponse>.Failure($"Connection error: {ex.Message}");
        }
    }

    private async Task<OperationResult<TResponse>> PutAsync<TRequest, TResponse>(string uri, TRequest request, string failureMessage, CancellationToken cancellationToken)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync(uri, request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken: cancellationToken);
            return ToOperationResult(result, failureMessage);
        }
        catch (Exception ex)
        {
            return OperationResult<TResponse>.Failure($"Connection error: {ex.Message}");
        }
    }

    private static OperationResult<T> ToOperationResult<T>(ApiResponse<T>? result, string failureMessage)
    {
        if (result is null || !result.Success || result.Data is null)
        {
            var message = result?.Message ?? failureMessage;
            if (result?.Errors?.Any() == true)
            {
                message += " " + string.Join(" ", result.Errors);
            }

            return OperationResult<T>.Failure(message);
        }

        return OperationResult<T>.Success(result.Data, result.Message ?? "Success.");
    }

    private static string BuildQueryString(IEnumerable<(string Key, string? Value)> values)
    {
        var pairs = values
            .Where(value => !string.IsNullOrWhiteSpace(value.Value))
            .Select(value => $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value!)}")
            .ToArray();
        return pairs.Length == 0 ? string.Empty : "?" + string.Join("&", pairs);
    }
}