using System.Net.Http.Json;
using TmsSystem.Application.Common;
using TmsSystem.BlazorWasm.Models;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.Services;

public sealed class ShipmentService(HttpClient httpClient)
{
    public Task<OperationResult<PagedResult<Shipment>>> GetShipmentsPageAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString([
            ("pageNumber", pageNumber.ToString()),
            ("pageSize", pageSize.ToString()),
            ("search", search),
            ("status", status)
        ]);
        return SendAsync<PagedResult<Shipment>>(() => httpClient.GetAsync($"api/shipment/paged{query}", cancellationToken), "Failed to retrieve shipments.", cancellationToken);
    }

    public async Task<OperationResult<List<Shipment>>> GetShipmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Shipment>>>("api/shipment", cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                return OperationResult<List<Shipment>>.Failure(result?.Message ?? "Failed to retrieve shipments.");
            }
            return OperationResult<List<Shipment>>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<List<Shipment>>.Failure($"Connection error: {ex.Message}");
        }
    }

    public async Task<OperationResult<Shipment>> GetShipmentByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<Shipment>>($"api/shipment/{id}", cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                return OperationResult<Shipment>.Failure(result?.Message ?? "Failed to retrieve shipment detail.");
            }
            return OperationResult<Shipment>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<Shipment>.Failure($"Connection error: {ex.Message}");
        }
    }

    // Related Master Data
    public async Task<List<Carrier>> GetCarriersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Carrier>>>("api/carrier", cancellationToken);
            return result?.Data ?? new List<Carrier>();
        }
        catch
        {
            return new List<Carrier>();
        }
    }

    public async Task<List<Vehicle>> GetVehiclesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Vehicle>>>("api/vehicle", cancellationToken);
            return result?.Data ?? new List<Vehicle>();
        }
        catch
        {
            return new List<Vehicle>();
        }
    }

    public async Task<List<Driver>> GetDriversAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<ApiResponse<List<Driver>>>("api/driver", cancellationToken);
            return result?.Data ?? new List<Driver>();
        }
        catch
        {
            return new List<Driver>();
        }
    }

    private static async Task<OperationResult<T>> SendAsync<T>(Func<Task<HttpResponseMessage>> send, string failureMessage, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await send();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                return OperationResult<T>.Failure(result?.Message ?? failureMessage);
            }

            return OperationResult<T>.Success(result.Data, result.Message ?? "Success.");
        }
        catch (Exception ex)
        {
            return OperationResult<T>.Failure($"Connection error: {ex.Message}");
        }
    }

    private static string BuildQueryString(IEnumerable<(string Key, string? Value)> values)
    {
        var pairs = values
            .Where(value => !string.IsNullOrWhiteSpace(value.Value))
            .Select(value => $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value!)}")
            .ToArray();
        return pairs.Length == 0 ? string.Empty : "?" + string.Join("&", pairs);
    }
}