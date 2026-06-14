using Microsoft.AspNetCore.Components;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class ShipmentDetailViewModel(ShipmentService shipmentService, TransportOrderService orderService, NavigationManager navigationManager)
{
    public Shipment? Shipment { get; private set; }
    public bool IsLoading { get; private set; } = true;

    // Resolved Names
    public string OrderNo { get; private set; } = "Loading...";
    public string CarrierName { get; private set; } = "Loading...";
    public string VehiclePlate { get; private set; } = "Loading...";
    public string VehicleType { get; private set; } = "...";
    public string DriverName { get; private set; } = "Loading...";

    public async Task InitializeAsync(long id)
    {
        IsLoading = true;

        var result = await shipmentService.GetShipmentByIdAsync(id);
        if (result.IsSuccess)
        {
            Shipment = result.Data;
            if (Shipment is not null)
            {
                // Fetch lookups
                if (Shipment.TransportOrderId.HasValue)
                {
                    var orderResult = await orderService.GetOrderByIdAsync(Shipment.TransportOrderId.Value);
                    OrderNo = orderResult.IsSuccess && orderResult.Data is not null ? orderResult.Data.TransportOrderNo : "Unknown Order";
                }

                var carriers = await shipmentService.GetCarriersAsync();
                var carrier = carriers.FirstOrDefault(c => c.CarrierId == Shipment.CarrierId);
                CarrierName = carrier?.CarrierName ?? "Not Assigned";

                var vehicles = await shipmentService.GetVehiclesAsync();
                var vehicle = vehicles.FirstOrDefault(v => v.VehicleId == Shipment.VehicleId);
                VehiclePlate = vehicle?.VehicleNo ?? "Not Assigned";
                VehicleType = vehicle?.VehicleType ?? "-";

                var drivers = await shipmentService.GetDriversAsync();
                var driver = drivers.FirstOrDefault(d => d.DriverId == Shipment.DriverId);
                DriverName = driver?.DriverName ?? "Not Assigned";
            }
        }

        IsLoading = false;
    }

    public string GetStatusBadgeClass(string status) => status switch
    {
        "Delivered" => "tms-badge-success",
        "Created" or "Packed" or "Loaded" => "bg-primary-subtle text-primary border border-primary-subtle",
        "Dispatched" => "tms-badge-warning",
        "Cancelled" => "bg-secondary-subtle text-secondary border border-secondary-subtle",
        _ => "bg-light text-dark border"
    };

    public void BackToList()
    {
        navigationManager.NavigateTo("shipments");
    }
}
