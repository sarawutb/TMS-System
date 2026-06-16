using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Dtos.Planning;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class PlanningWorkbenchViewModel(PlanningService planningService, NavigationManager navigationManager)
{
    public PlanningWorkbenchDto Workbench { get; private set; } = new();
    public HashSet<long> SelectedOrderIds { get; } = new();
    public bool IsLoading { get; private set; } = true;
    public bool IsSolving { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;
    public PaginationState AvailableOrdersPagination { get; } = new();
    public PaginationState RoutePlansPagination { get; } = new();

    public IReadOnlyList<PlanningOrderDto> AvailableOrders => Workbench.AvailableOrdersPage.Items;
    public IReadOnlyList<RoutePlanSummaryDto> RoutePlans => Workbench.RoutePlansPage.Items;
    public IReadOnlyList<LoadPlanSummaryDto> LoadPlans => Workbench.LoadPlans;
    public IReadOnlyList<PlanningOrderDto> PagedAvailableOrders => AvailableOrders;
    public IReadOnlyList<RoutePlanSummaryDto> PagedRoutePlans => RoutePlans;
    public decimal SelectedWeightKg => AvailableOrders.Where(order => SelectedOrderIds.Contains(order.TransportOrderId)).Sum(order => order.TotalWeightKg);
    public decimal SelectedVolumeM3 => AvailableOrders.Where(order => SelectedOrderIds.Contains(order.TransportOrderId)).Sum(order => order.TotalVolumeM3);

    public async Task InitializeAsync() => await LoadAsync();

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        var result = await planningService.GetWorkbenchAsync(AvailableOrdersPagination.CurrentPage, RoutePlansPagination.CurrentPage, AvailableOrdersPagination.PageSize);
        if (result.IsSuccess && result.Data is not null)
        {
            Workbench = result.Data;
            AvailableOrdersPagination.ApplyMetadata(result.Data.AvailableOrdersPage);
            RoutePlansPagination.ApplyMetadata(result.Data.RoutePlansPage);
        }
        else
        {
            ErrorMessage = result.Message;
            AvailableOrdersPagination.SetTotal(0);
            RoutePlansPagination.SetTotal(0);
        }

        IsLoading = false;
    }

    public void ToggleOrder(long orderId, bool selected)
    {
        if (selected)
        {
            SelectedOrderIds.Add(orderId);
        }
        else
        {
            SelectedOrderIds.Remove(orderId);
        }
    }

    public void ClearSelection() => SelectedOrderIds.Clear();

    public async Task CreateRoutePlanAsync()
    {
        if (SelectedOrderIds.Count == 0)
        {
            ErrorMessage = "Select at least one order before solving a route.";
            return;
        }

        IsSolving = true;
        ErrorMessage = string.Empty;
        var result = await planningService.SolveRouteAsync(new RoutePlanningRequestDto
        {
            TransportOrderIds = SelectedOrderIds.ToArray(),
            PlanDate = DateTime.UtcNow,
            TransportMode = "Road",
            OptimizationEngine = "Heuristic"
        });

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Message;
        }
        else
        {
            SelectedOrderIds.Clear();
            await LoadAsync();
        }

        IsSolving = false;
    }

    public async Task CreateLoadPlanAsync(RoutePlanSummaryDto routePlan)
    {
        var orderIds = routePlan.Stops
            .Where(stop => stop.TransportOrderId.HasValue)
            .Select(stop => stop.TransportOrderId!.Value)
            .Distinct()
            .ToArray();
        if (orderIds.Length == 0)
        {
            ErrorMessage = "Route has no order stops to load.";
            return;
        }

        IsSolving = true;
        ErrorMessage = string.Empty;
        var result = await planningService.SolveLoadAsync(new LoadPlanningRequestDto
        {
            RoutePlanId = routePlan.RoutePlanId,
            TransportOrderIds = orderIds,
            VehicleType = string.IsNullOrWhiteSpace(routePlan.VehicleType) ? "Truck" : routePlan.VehicleType
        });

        if (!result.IsSuccess)
        {
            ErrorMessage = result.Message;
        }
        else
        {
            await LoadAsync();
        }

        IsSolving = false;
    }

    public void ViewRoute(long routePlanId) => navigationManager.NavigateTo($"planning/routes/{routePlanId}");
    public void ViewLoad(long loadPlanId) => navigationManager.NavigateTo($"planning/loads/{loadPlanId}");

    public string GetStatusBadgeClass(string status) => status switch
    {
        "Approved" => "tms-badge-success",
        "ManualAdjusted" => "tms-badge-warning",
        "Proposed" => "bg-primary-subtle text-primary border border-primary-subtle",
        _ => "bg-light text-dark border"
    };
}