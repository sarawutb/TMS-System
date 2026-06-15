using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class LoadPlanDetailViewModel(PlanningService planningService, NavigationManager navigationManager)
{
    public LoadPlanSummaryDto? LoadPlan { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public string ErrorMessage { get; private set; } = string.Empty;

    public async Task InitializeAsync(long loadPlanId)
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        var result = await planningService.GetLoadPlanAsync(loadPlanId);
        if (result.IsSuccess && result.Data is not null)
        {
            LoadPlan = result.Data;
        }
        else
        {
            ErrorMessage = result.Message;
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
