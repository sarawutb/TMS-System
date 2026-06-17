using System.Net.Http.Json;
using TmsSystem.Application.Common;
using TmsSystem.BlazorWasm.Models;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.Services;

public sealed class MasterDataService(HttpClient httpClient)
{
    public Task<OperationResult<List<Factory>>> GetFactoriesAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Factory>("api/factory", "factories", cancellationToken);

    public Task<OperationResult<List<Customer>>> GetCustomersAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Customer>("api/customer", "customers", cancellationToken);

    public Task<OperationResult<List<Location>>> GetLocationsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Location>("api/location", "locations", cancellationToken);

    public Task<OperationResult<List<Carrier>>> GetCarriersAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Carrier>("api/carrier", "carriers", cancellationToken);

    public Task<OperationResult<List<Vehicle>>> GetVehiclesAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Vehicle>("api/vehicle", "vehicles", cancellationToken);

    public Task<OperationResult<List<Driver>>> GetDriversAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Driver>("api/driver", "drivers", cancellationToken);

    public Task<OperationResult<List<ProductProfile>>> GetProductProfilesAsync(CancellationToken cancellationToken = default)
        => GetListAsync<ProductProfile>("api/product-profile", "product profiles", cancellationToken);

    public Task<OperationResult<List<ProductGroup>>> GetProductGroupsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<ProductGroup>("api/product-group", "product groups", cancellationToken);

    public Task<OperationResult<List<ProductCategory>>> GetProductCategoriesAsync(CancellationToken cancellationToken = default)
        => GetListAsync<ProductCategory>("api/product-category", "product categories", cancellationToken);

    public Task<OperationResult<List<Unit>>> GetUnitsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Unit>("api/unit", "units", cancellationToken);

    public Task<OperationResult<List<Product>>> GetProductsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Product>("api/product", "products", cancellationToken);

    public Task<OperationResult<List<ProductUnit>>> GetProductUnitsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<ProductUnit>("api/product-unit", "product units", cancellationToken);

    public Task<OperationResult<List<Province>>> GetProvincesAsync(CancellationToken cancellationToken = default)
        => GetListAsync<Province>("api/province", "provinces", cancellationToken);

    public Task<OperationResult<List<District>>> GetDistrictsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<District>("api/district", "districts", cancellationToken);

    public Task<OperationResult<List<District>>> GetDistrictsByProvinceAsync(long provinceId, CancellationToken cancellationToken = default)
        => GetListAsync<District>($"api/district/by-province/{provinceId}", "districts", cancellationToken);

    public Task<OperationResult<List<SubDistrict>>> GetSubDistrictsAsync(CancellationToken cancellationToken = default)
        => GetListAsync<SubDistrict>("api/subdistrict", "sub-districts", cancellationToken);

    public Task<OperationResult<List<SubDistrict>>> GetSubDistrictsByDistrictAsync(long districtId, CancellationToken cancellationToken = default)
        => GetListAsync<SubDistrict>($"api/subdistrict/by-district/{districtId}", "sub-districts", cancellationToken);

    public Task<OperationResult<List<T>>> GetListAsync<T>(string endpoint, string label, CancellationToken cancellationToken = default)
        => SendAsync<List<T>>(() => httpClient.GetAsync(endpoint, cancellationToken), $"Failed to retrieve {label}.", cancellationToken);

    public Task<OperationResult<PagedResult<T>>> GetPagedListAsync<T>(string endpoint, string label, int pageNumber, int pageSize, string? search = null, CancellationToken cancellationToken = default)
    {
        var query = BuildQueryString([
            ("pageNumber", pageNumber.ToString()),
            ("pageSize", pageSize.ToString()),
            ("search", search)
        ]);
        return SendAsync<PagedResult<T>>(() => httpClient.GetAsync($"{endpoint}/paged{query}", cancellationToken), $"Failed to retrieve {label}.", cancellationToken);
    }

    public Task<OperationResult<T>> GetByIdAsync<T>(string endpoint, long id, CancellationToken cancellationToken = default)
        => SendAsync<T>(() => httpClient.GetAsync($"{endpoint}/{id}", cancellationToken), $"Failed to retrieve {typeof(T).Name}.", cancellationToken);

    public Task<OperationResult<T>> CreateAsync<T>(string endpoint, T entity, CancellationToken cancellationToken = default)
        => SendAsync<T>(() => httpClient.PostAsJsonAsync(endpoint, entity, cancellationToken), $"Failed to create {typeof(T).Name}.", cancellationToken);

    public Task<OperationResult<T>> UpdateAsync<T>(string endpoint, long id, T entity, CancellationToken cancellationToken = default)
        => SendAsync<T>(() => httpClient.PutAsJsonAsync($"{endpoint}/{id}", entity, cancellationToken), $"Failed to update {typeof(T).Name}.", cancellationToken);

    public async Task<OperationResult<bool>> DeleteAsync(string endpoint, long id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await httpClient.DeleteAsync($"{endpoint}/{id}", cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>(cancellationToken: cancellationToken);
            if (result is null || !result.Success)
            {
                return OperationResult<bool>.Failure(BuildError(result?.Message ?? "Failed to delete master data.", result?.Errors));
            }

            return OperationResult<bool>.Success(true, result.Message ?? "Deleted.");
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure($"Connection error: {ex.Message}");
        }
    }

    private static async Task<OperationResult<T>> SendAsync<T>(
        Func<Task<HttpResponseMessage>> send,
        string failureMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await send();
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);
            if (result is null || !result.Success || result.Data is null)
            {
                return OperationResult<T>.Failure(BuildError(result?.Message ?? failureMessage, result?.Errors));
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

    private static string BuildError(string message, IEnumerable<string>? errors)
    {
        var details = errors?.Where(error => !string.IsNullOrWhiteSpace(error)).ToArray();
        return details is { Length: > 0 } ? $"{message} {string.Join(" ", details)}" : message;
    }
}

