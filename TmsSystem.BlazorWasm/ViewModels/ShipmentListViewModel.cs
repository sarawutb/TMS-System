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

    public string SearchQuery { get; set; } = string.Empty;
    public string SelectedStatus { get; set; } = string.Empty;
    public bool IsLoading { get; private set; } = true;

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        IsLoading = true;

        var shipmentsResult = await shipmentService.GetShipmentsAsync();
        if (shipmentsResult.IsSuccess)
        {
            Shipments = shipmentsResult.Data ?? new();
        }

        // Fetch lookups
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

        IsLoading = false;
    }

    public List<Shipment> FilteredShipments => Shipments
        .Where(s => string.IsNullOrWhiteSpace(SearchQuery) || s.ShipmentNo.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase))
        .Where(s => string.IsNullOrWhiteSpace(SelectedStatus) || s.ShipmentStatus == SelectedStatus)
        .ToList();

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

    public void ClearFilters()
    {
        SearchQuery = string.Empty;
        SelectedStatus = string.Empty;
    }

    public void ViewDetail(long id)
    {
        navigationManager.NavigateTo($"shipments/{id}");
    }
}
