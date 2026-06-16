using Microsoft.AspNetCore.Components;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class ShipmentListViewModel(ShipmentService shipmentService, TransportOrderService orderService, NavigationManager navigationManager)
{
    public List<Shipment> Shipments { get; private set; } = new();
    public Dictionary<long, string> OrderLookup { get; private set; } = new();
    public Dictionary<long, string> CarrierLookup { get; private set; } = new();
    public Dictionary<long, string> VehicleLookup { get; private set; } = new();
    public Dictionary<long, string> DriverLookup { get; private set; } = new();

    private string searchQuery = string.Empty;
    private string selectedStatus = string.Empty;

    public string SearchQuery
    {
        get => searchQuery;
        set => searchQuery = value;
    }

    public string SelectedStatus
    {
        get => selectedStatus;
        set => selectedStatus = value;
    }

    public bool IsLoading { get; private set; } = true;
    public PaginationState Pagination { get; } = new();

    public async Task InitializeAsync()
    {
        await LoadLookupDataAsync();
        await LoadPageAsync();
    }

    public async Task LoadPageAsync()
    {
        IsLoading = true;
        var shipmentsResult = await shipmentService.GetShipmentsPageAsync(Pagination.CurrentPage, Pagination.PageSize, SearchQuery, SelectedStatus);
        if (shipmentsResult.IsSuccess && shipmentsResult.Data is not null)
        {
            Shipments = shipmentsResult.Data.Items.ToList();
            Pagination.ApplyMetadata(shipmentsResult.Data);
        }
        else
        {
            Shipments = new();
            Pagination.SetTotal(0);
        }

        IsLoading = false;
    }

    public async Task LoadFirstPageAsync()
    {
        Pagination.Reset();
        await LoadPageAsync();
    }

    public List<Shipment> FilteredShipments => Shipments;
    public IReadOnlyList<Shipment> PagedShipments => Shipments;

    public string GetOrderNo(long id) => OrderLookup.TryGetValue(id, out var orderNo) ? orderNo : $"#{id}";
    public string GetCarrierName(long? id) => id.HasValue && CarrierLookup.TryGetValue(id.Value, out var name) ? name : "Not Assigned";
    public string GetVehiclePlate(long? id) => id.HasValue && VehicleLookup.TryGetValue(id.Value, out var plate) ? plate : "Not Assigned";
    public string GetDriverName(long? id) => id.HasValue && DriverLookup.TryGetValue(id.Value, out var name) ? name : "Not Assigned";

    public string GetStatusBadgeClass(string status) => status switch
    {
        "Delivered" => "tms-badge-success",
        "Created" or "Packed" or "Loaded" => "bg-primary-subtle text-primary border border-primary-subtle",
        "Dispatched" => "tms-badge-warning",
        "Cancelled" => "bg-secondary-subtle text-secondary border border-secondary-subtle",
        _ => "bg-light text-dark border"
    };

    public async Task ClearFiltersAsync()
    {
        SearchQuery = string.Empty;
        SelectedStatus = string.Empty;
        await LoadFirstPageAsync();
    }

    public void ViewDetail(long id)
    {
        navigationManager.NavigateTo($"shipments/{id}");
    }

    private async Task LoadLookupDataAsync()
    {
        var ordersResult = await orderService.GetOrdersAsync();
        if (ordersResult.IsSuccess && ordersResult.Data is not null)
        {
            OrderLookup = ordersResult.Data.ToDictionary(o => o.TransportOrderId, o => o.TransportOrderNo);
        }

        var carriers = await shipmentService.GetCarriersAsync();
        CarrierLookup = carriers.ToDictionary(c => c.CarrierId, c => c.CarrierName);

        var vehicles = await shipmentService.GetVehiclesAsync();
        VehicleLookup = vehicles.ToDictionary(v => v.VehicleId, v => $"{v.VehicleNo} ({v.VehicleType})");

        var drivers = await shipmentService.GetDriversAsync();
        DriverLookup = drivers.ToDictionary(d => d.DriverId, d => d.DriverName);
    }
}