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

    private string searchQuery = string.Empty;
    private string selectedPriority = string.Empty;
    private string selectedStatus = string.Empty;

    public string SearchQuery
    {
        get => searchQuery;
        set => searchQuery = value;
    }

    public string SelectedPriority
    {
        get => selectedPriority;
        set => selectedPriority = value;
    }

    public string SelectedStatus
    {
        get => selectedStatus;
        set => selectedStatus = value;
    }

    public bool IsLoading { get; private set; } = true;
    public PaginationState Pagination { get; } = new();

    // Delete Modal State
    public bool ShowDeleteModal { get; private set; }
    public long DeleteOrderId { get; private set; }
    public string DeleteOrderNo { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await LoadLookupDataAsync();
        await LoadPageAsync();
    }

    public async Task LoadPageAsync()
    {
        IsLoading = true;
        var ordersResult = await orderService.GetOrdersPageAsync(Pagination.CurrentPage, Pagination.PageSize, SearchQuery, SelectedPriority, SelectedStatus);
        if (ordersResult.IsSuccess && ordersResult.Data is not null)
        {
            Orders = ordersResult.Data.Items.ToList();
            Pagination.ApplyMetadata(ordersResult.Data);
        }
        else
        {
            Orders = new();
            Pagination.SetTotal(0);
        }

        IsLoading = false;
    }

    public async Task LoadFirstPageAsync()
    {
        Pagination.Reset();
        await LoadPageAsync();
    }

    public List<TransportOrder> FilteredOrders => Orders;
    public IReadOnlyList<TransportOrder> PagedOrders => Orders;

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

    public async Task ClearFiltersAsync()
    {
        SearchQuery = string.Empty;
        SelectedPriority = string.Empty;
        SelectedStatus = string.Empty;
        await LoadFirstPageAsync();
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
            await LoadPageAsync();
        }
    }

    private async Task LoadLookupDataAsync()
    {
        var customers = await orderService.GetCustomersAsync();
        CustomerLookup = customers.ToDictionary(c => c.CustomerId, c => c.CustomerName);

        var factories = await orderService.GetFactoriesAsync();
        FactoryLookup = factories.ToDictionary(f => f.FactoryId, f => f.FactoryName);

        var locations = await orderService.GetLocationsAsync();
        LocationLookup = locations.ToDictionary(l => l.LocationId, l => $"{l.LocationName} ({l.LocationCode})");
    }
}