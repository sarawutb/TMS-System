using System.Net.Http.Json;
using TmsSystem.Application.Common;
using TmsSystem.BlazorWasm.Models;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.Services;

public sealed class TransportOrderService(HttpClient httpClient)
{
    public async Task<OperationResult<List<TransportOrder>>> GetOrdersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<TransportOrder>>>("api/transport-order", cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                return OperationResult<List<TransportOrder>>.Failure(result?.Message ?? "Failed to retrieve transport orders.");
            }
            return OperationResult<List<TransportOrder>>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<List<TransportOrder>>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<OperationResult<TransportOrder>> GetOrderByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<TransportOrder>>($"api/transport-order/{id}", cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                return OperationResult<TransportOrder>.Failure(result?.Message ?? "Failed to retrieve transport order detail.");
            }
            return OperationResult<TransportOrder>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<TransportOrder>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<OperationResult<TransportOrder>> CreateOrderAsync(TransportOrder order, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/transport-order", order, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransportOrder>>(cancellationToken: cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                var error = result?.Message ?? "Failed to create transport order.";
                if (result?.Errors?.Any() == true)
                {
                    error += " " + string.Join(" ", result.Errors);
                }
                return OperationResult<TransportOrder>.Failure(error);
            }
            return OperationResult<TransportOrder>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<TransportOrder>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<OperationResult<TransportOrder>> UpdateOrderAsync(long id, TransportOrder order, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PutAsJsonAsync($"api/transport-order/{id}", order, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<TransportOrder>>(cancellationToken: cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                var error = result?.Message ?? "Failed to update transport order.";
                if (result?.Errors?.Any() == true)
                {
                    error += " " + string.Join(" ", result.Errors);
                }
                return OperationResult<TransportOrder>.Failure(error);
            }
            return OperationResult<TransportOrder>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<TransportOrder>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<OperationResult<bool>> DeleteOrderAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.DeleteAsync($"api/transport-order/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            if (result is null || !result.Success)
            {
                return OperationResult<bool>.Failure(result?.Message ?? "Failed to delete transport order.");
            }
            return OperationResult<bool>.Success(true, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure($"Connection error: {ex.Message}");
        }
    }

    // Master Data Retrieval
    public async Task<List<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Customer>>>("api/customer", cancellationToken);
            return result?.Data ?? new List<Customer>();
        }
        catch
        {
            return new List<Customer>();
        }
    }

    public async Task<List<Factory>> GetFactoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Factory>>>("api/factory", cancellationToken);
            return result?.Data ?? new List<Factory>();
        }
        catch
        {
            return new List<Factory>();
        }
    }

    public async Task<List<Location>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Location>>>("api/location", cancellationToken);
            return result?.Data ?? new List<Location>();
        }
        catch
        {
            return new List<Location>();
        }
    }
}
