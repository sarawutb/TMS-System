using Microsoft.AspNetCore.Components;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class OrderDetailViewModel(TransportOrderService orderService, NavigationManager navigationManager)
{
    public TransportOrder? Order { get; private set; }
    public bool IsLoading { get; private set; } = true;

    // Resolved Names
    public string CustomerName { get; private set; } = "Loading...";
    public string CustomerCode { get; private set; } = "...";
    public string FactoryName { get; private set; } = "Loading...";
    public string FactoryCode { get; private set; } = "...";
    
    public string PickupName { get; private set; } = "Loading...";
    public string PickupCode { get; private set; } = "...";
    public string PickupAddress { get; private set; } = "...";

    public string DeliveryName { get; private set; } = "Loading...";
    public string DeliveryCode { get; private set; } = "...";
    public string DeliveryAddress { get; private set; } = "...";

    public bool ShowDeleteModal { get; private set; }

    public async Task InitializeAsync(long id)
    {
        IsLoading = true;
        ShowDeleteModal = false;

        var result = await orderService.GetOrderByIdAsync(id);
        if (result.IsSuccess)
        {
            Order = result.Data;
            if (Order is not null)
            {
                // Fetch Master Data references
                var customers = await orderService.GetCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.CustomerId == Order.CustomerId);
                CustomerName = customer?.CustomerName ?? "Unknown Customer";
                CustomerCode = customer?.CustomerCode ?? "-";

                var factories = await orderService.GetFactoriesAsync();
                var factory = factories.FirstOrDefault(f => f.FactoryId == Order.FactoryId);
                FactoryName = factory?.FactoryName ?? "Unknown Factory";
                FactoryCode = factory?.FactoryCode ?? "-";

                var locations = await orderService.GetLocationsAsync();
                
                var pickup = locations.FirstOrDefault(l => l.LocationId == Order.PickupLocationId);
                PickupName = pickup?.LocationName ?? "Unknown Location";
                PickupCode = pickup?.LocationCode ?? "-";
                PickupAddress = pickup?.AddressText ?? "No Address details.";

                var delivery = locations.FirstOrDefault(l => l.LocationId == Order.DeliveryLocationId);
                DeliveryName = delivery?.LocationName ?? "Unknown Location";
                DeliveryCode = delivery?.LocationCode ?? "-";
                DeliveryAddress = delivery?.AddressText ?? "No Address details.";
            }
        }

        IsLoading = false;
    }

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

    public void BackToList()
    {
        navigationManager.NavigateTo("transport-orders");
    }

    public void EditOrder(long id)
    {
        navigationManager.NavigateTo($"transport-orders/{id}/edit");
    }

    public void ConfirmDelete()
    {
        ShowDeleteModal = true;
    }

    public void CloseDeleteModal()
    {
        ShowDeleteModal = false;
    }

    public async Task ExecuteDeleteAsync(long id)
    {
        ShowDeleteModal = false;
        var result = await orderService.DeleteOrderAsync(id);
        if (result.IsSuccess)
        {
            navigationManager.NavigateTo("transport-orders");
        }
    }
}
