using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Common;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class OrderFormViewModel(TransportOrderService orderService, NavigationManager navigationManager)
{
    public TransportOrder Order { get; private set; } = new();
    public List<Customer> Customers { get; private set; } = new();
    public List<Factory> Factories { get; private set; } = new();
    public List<Location> Locations { get; private set; } = new();

    public bool IsLoading { get; private set; } = true;
    public bool IsSaving { get; private set; }
    public string? ErrorMessage { get; set; }

    public string FormattedPickupDate { get; set; } = string.Empty;
    public string FormattedDeliveryDate { get; set; } = string.Empty;

    public async Task InitializeAsync(long? id)
    {
        IsLoading = true;
        ErrorMessage = null;

        // Fetch lookups
        Factories = await orderService.GetFactoriesAsync();
        Customers = await orderService.GetCustomersAsync();
        Locations = await orderService.GetLocationsAsync();

        if (id.HasValue)
        {
            var result = await orderService.GetOrderByIdAsync(id.Value);
            if (result.IsSuccess && result.Data is not null)
            {
                Order = result.Data;
            }
            else
            {
                ErrorMessage = result.Message ?? "Failed to fetch order details.";
            }
        }
        else
        {
            // Initialize with defaults for new order
            Order = new()
            {
                TransportOrderNo = $"TO-{new Random().Next(100000, 999999)}",
                SourceSystem = "TMS_PORTAL",
                Priority = "Medium",
                Status = "New",
                RequestedPickupDate = DateTime.Now,
                RequestedDeliveryDate = DateTime.Now.AddDays(1)
            };
        }

        // Initialize formatted strings for inputs
        FormattedPickupDate = Order.RequestedPickupDate?.ToString("yyyy-MM-ddTHH:mm") ?? string.Empty;
        FormattedDeliveryDate = Order.RequestedDeliveryDate?.ToString("yyyy-MM-ddTHH:mm") ?? string.Empty;

        IsLoading = false;
    }

    public void OnPickupDateChanged(string val)
    {
        FormattedPickupDate = val;
        if (DateTime.TryParse(FormattedPickupDate, out var date))
        {
            Order.RequestedPickupDate = date;
        }
        else
        {
            Order.RequestedPickupDate = null;
        }
    }

    public void OnDeliveryDateChanged(string val)
    {
        FormattedDeliveryDate = val;
        if (DateTime.TryParse(FormattedDeliveryDate, out var date))
        {
            Order.RequestedDeliveryDate = date;
        }
        else
        {
            Order.RequestedDeliveryDate = null;
        }
    }

    public async Task SaveOrderAsync(long? id)
    {
        if (Order.FactoryId == 0 || !Order.CustomerId.HasValue || Order.CustomerId.Value == 0 ||
            Order.PickupLocationId == 0 || Order.DeliveryLocationId == 0)
        {
            ErrorMessage = "Please complete all required selections (Factory, Customer, Pickup, Delivery).";
            return;
        }

        IsSaving = true;
        ErrorMessage = null;

        OperationResult<TransportOrder> result;
        if (id.HasValue)
        {
            result = await orderService.UpdateOrderAsync(id.Value, Order);
        }
        else
        {
            result = await orderService.CreateOrderAsync(Order);
        }

        if (result.IsSuccess)
        {
            navigationManager.NavigateTo("transport-orders");
        }
        else
        {
            ErrorMessage = result.Message;
        }

        IsSaving = false;
    }

    public void Cancel(long? id)
    {
        navigationManager.NavigateTo(id.HasValue ? $"transport-orders/{id}" : "transport-orders");
    }
}
