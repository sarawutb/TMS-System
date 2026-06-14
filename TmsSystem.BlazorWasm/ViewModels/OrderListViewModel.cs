using Microsoft.AspNetCore.Components;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class OrderListViewModel(TransportOrderService orderService, NavigationManager navigationManager)
{
    public List<TransportOrder> Orders { get; private set; } = new();
    public Dictionary<long, string> CustomerLookup { get; private set; } = new();
    public Dictionary<long, string> FactoryLookup { get; private set; } = new();
    public Dictionary<long, string> LocationLookup { get; private set; } = new();

    public string SearchQuery { get; set; } = string.Empty;
    public string SelectedPriority { get; set; } = string.Empty;
    public string SelectedStatus { get; set; } = string.Empty;
    public bool IsLoading { get; private set; } = true;

    // Delete Modal State
    public bool ShowDeleteModal { get; private set; }
    public long DeleteOrderId { get; private set; }
    public string DeleteOrderNo { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;

        var ordersResult = await orderService.GetOrdersAsync();
        if (ordersResult.IsSuccess)
        {
            Orders = ordersResult.Data ?? new();
        }

        // Fetch lookups
        var customers = await orderService.GetCustomersAsync();
        CustomerLookup = customers.ToDictionary(c => c.CustomerId, c => c.CustomerName);

        var factories = await orderService.GetFactoriesAsync();
        FactoryLookup = factories.ToDictionary(f => f.FactoryId, f => f.FactoryName);

        var locations = await orderService.GetLocationsAsync();
        LocationLookup = locations.ToDictionary(l => l.LocationId, l => $"{l.LocationName} ({l.LocationCode})");

        IsLoading = false;
    }

    public List<TransportOrder> FilteredOrders => Orders
        .Where(o => string.IsNullOrWhiteSpace(SearchQuery) || o.TransportOrderNo.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        .Where(o => string.IsNullOrWhiteSpace(SelectedPriority) || o.Priority == SelectedPriority)
        .Where(o => string.IsNullOrWhiteSpace(SelectedStatus) || o.Status == SelectedStatus)
        .ToList();

    public string GetCustomerName(long? id) => id.HasValue && CustomerLookup.TryGetValue(id.Value, out var name) ? name : "Unknown Customer";
    public string GetFactoryName(long id) => FactoryLookup.TryGetValue(id, out var name) ? name : "Unknown Factory";
    public string GetLocationName(long id) => LocationLookup.TryGetValue(id, out var name) ? name : "Unknown Location";

    public string GetPriorityBadgeClass(string priority) => priority switch
    {
        "High" => "tms-badge-danger",
        "Medium" => "tms-badge-warning",
        "Low" => "tms-badge-success",
        _ => "bg-light text-dark border"
    };

    public string GetStatusBadgeClass(string status) => status switch
    {
        "Completed" => "tms-badge-success",
        "New" or "Draft" => "bg-primary-subtle text-primary border border-primary-subtle",
        "Dispatched" => "tms-badge-warning",
        "Cancelled" => "bg-secondary-subtle text-secondary border border-secondary-subtle",
        _ => "bg-light text-dark border"
    };

    public void ClearFilters()
    {
        SearchQuery = string.Empty;
        SelectedPriority = string.Empty;
        SelectedStatus = string.Empty;
    }

    public void CreateNewOrder()
    {
        navigationManager.NavigateTo("transport-orders/new");
    }

    public void ViewDetail(long id)
    {
        navigationManager.NavigateTo($"transport-orders/{id}");
    }

    public void EditOrder(long id)
    {
        navigationManager.NavigateTo($"transport-orders/{id}/edit");
    }

    public void ConfirmDelete(long id, string orderNo)
    {
        DeleteOrderId = id;
        DeleteOrderNo = orderNo;
        ShowDeleteModal = true;
    }

    public void CloseDeleteModal()
    {
        ShowDeleteModal = false;
    }

    public async Task ExecuteDeleteAsync()
    {
        ShowDeleteModal = false;
        var result = await orderService.DeleteOrderAsync(DeleteOrderId);
        if (result.IsSuccess)
        {
            Orders.RemoveAll(o => o.TransportOrderId == DeleteOrderId);
        }
    }
}
