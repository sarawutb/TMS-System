using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Common;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class RoutePlanDetailViewModel(PlanningService planningService, NavigationManager navigationManager)
{
    public RoutePlanSummaryDto? RoutePlan { get; private set; }
    public List<RouteStopDto> EditableStops { get; private set; } = new();
    public bool IsLoading { get; private set; } = true;
    public bool IsSaving { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public PaginationState StopsPagination { get; } = new();
    public IReadOnlyList<RouteStopDto> PagedEditableStops => RoutePlan?.StopsPage.Items ?? Array.Empty<RouteStopDto>();

    public async Task InitializeAsync(long routePlanId)
    {
        StopsPagination.Reset();
        await LoadPageAsync(routePlanId);
    }

    public async Task LoadPageAsync(long? routePlanId = null)
    {
        var targetRoutePlanId = routePlanId ?? RoutePlan?.RoutePlanId;
        if (targetRoutePlanId is null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        var result = await planningService.GetRoutePlanAsync(targetRoutePlanId.Value, StopsPagination.CurrentPage, StopsPagination.PageSize);
        if (result.IsSuccess && result.Data is not null)
        {
            RoutePlan = result.Data;
            EditableStops = result.Data.Stops.OrderBy(stop => stop.Sequence).ToList();
            StopsPagination.ApplyMetadata(result.Data.StopsPage);
        }
        else
        {
            ErrorMessage = result.Message;
            StopsPagination.SetTotal(0);
        }

        IsLoading = false;
    }

    public void MoveStopUp(int index)
    {
        if (index <= 0 || index >= EditableStops.Count)
        {
            return;
        }

        (EditableStops[index - 1], EditableStops[index]) = (EditableStops[index], EditableStops[index - 1]);
        Resequence();
    }

    public void MoveStopDown(int index)
    {
        if (index < 0 || index >= EditableStops.Count - 1)
        {
            return;
        }

        (EditableStops[index + 1], EditableStops[index]) = (EditableStops[index], EditableStops[index + 1]);
        Resequence();
    }

    public async Task SaveStopOrderAsync()
    {
        if (RoutePlan is null)
        {
            return;
        }

        IsSaving = true;
        ErrorMessage = string.Empty;
        var result = await planningService.UpdateRouteStopsAsync(RoutePlan.RoutePlanId, EditableStops, StopsPagination.CurrentPage, StopsPagination.PageSize);
        if (result.IsSuccess && result.Data is not null)
        {
            RoutePlan = result.Data;
            EditableStops = result.Data.Stops.OrderBy(stop => stop.Sequence).ToList();
            StopsPagination.ApplyMetadata(result.Data.StopsPage);
        }
        else
        {
            ErrorMessage = result.Message;
        }

        IsSaving = false;
    }

    public async Task ApproveAsync()
    {
        if (RoutePlan is null)
        {
            return;
        }

        IsSaving = true;
        ErrorMessage = string.Empty;
        var result = await planningService.ApproveRouteAsync(RoutePlan.RoutePlanId);
        if (result.IsSuccess && result.Data is not null)
        {
            RoutePlan = result.Data with
            {
                StopsPage = PagedResult<RouteStopDto>.Create(EditableStops, StopsPagination.CurrentPage, StopsPagination.PageSize)
            };
        }
        else
        {
            ErrorMessage = result.Message;
        }

        IsSaving = false;
    }

    public async Task CreateLoadPlanAsync()
    {
        if (RoutePlan is null)
        {
            return;
        }

        var orderIds = RoutePlan.Stops.Where(stop => stop.TransportOrderId.HasValue).Select(stop => stop.TransportOrderId!.Value).Distinct().ToArray();
        IsSaving = true;
        ErrorMessage = string.Empty;
        var result = await planningService.SolveLoadAsync(new LoadPlanningRequestDto
        {
            RoutePlanId = RoutePlan.RoutePlanId,
            TransportOrderIds = orderIds,
            VehicleType = string.IsNullOrWhiteSpace(RoutePlan.VehicleType) ? "Truck" : RoutePlan.VehicleType
        });

        if (result.IsSuccess && result.Data is not null)
        {
            navigationManager.NavigateTo($"planning/loads/{result.Data.LoadPlanId}");
        }
        else
        {
            ErrorMessage = result.Message;
        }

        IsSaving = false;
    }

    public void BackToWorkbench() => navigationManager.NavigateTo("planning");

    private void Resequence()
    {
        EditableStops = EditableStops.Select((stop, index) => stop with { Sequence = index + 1 }).ToList();
        if (RoutePlan is not null)
        {
            RoutePlan = RoutePlan with
            {
                Stops = EditableStops,
                StopsPage = PagedResult<RouteStopDto>.Create(EditableStops, StopsPagination.CurrentPage, StopsPagination.PageSize)
            };
            StopsPagination.ApplyMetadata(RoutePlan.StopsPage);
        }
    }
}