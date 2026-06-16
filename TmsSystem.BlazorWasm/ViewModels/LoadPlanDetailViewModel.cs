using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class LoadPlanDetailViewModel(PlanningService planningService, NavigationManager navigationManager)
{
    public LoadPlanSummaryDto? LoadPlan { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public string ErrorMessage { get; private set; } = string.Empty;
    public PaginationState PlacementsPagination { get; } = new();
    public IReadOnlyList<CargoPlacementDto> PagedPlacements => LoadPlan?.PlacementsPage.Items ?? Array.Empty<CargoPlacementDto>();

    public async Task InitializeAsync(long loadPlanId)
    {
        PlacementsPagination.Reset();
        await LoadPageAsync(loadPlanId);
    }

    public async Task LoadPageAsync(long? loadPlanId = null)
    {
        var targetLoadPlanId = loadPlanId ?? LoadPlan?.LoadPlanId;
        if (targetLoadPlanId is null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        var result = await planningService.GetLoadPlanAsync(targetLoadPlanId.Value, PlacementsPagination.CurrentPage, PlacementsPagination.PageSize);
        if (result.IsSuccess && result.Data is not null)
        {
            LoadPlan = result.Data;
            PlacementsPagination.ApplyMetadata(result.Data.PlacementsPage);
        }
        else
        {
            ErrorMessage = result.Message;
            PlacementsPagination.SetTotal(0);
        }

        IsLoading = false;
    }

    public void BackToWorkbench() => navigationManager.NavigateTo("planning");
    public void ViewRoute()
    {
        if (LoadPlan?.RoutePlanId is long routePlanId)
        {
            navigationManager.NavigateTo($"planning/routes/{routePlanId}");
        }
    }
}