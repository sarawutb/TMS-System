using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Common;
using TmsSystem.BlazorWasm.Models;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class MasterDataListViewModel(MasterDataService masterDataService, NavigationManager navigationManager)
{
    public IReadOnlyList<MasterDataDefinition> Definitions => MasterDataDefinitions.All;
    public MasterDataDefinition SelectedDefinition { get; private set; } = MasterDataDefinitions.Default;
    public List<object> Items { get; private set; } = new();
    public Dictionary<string, Dictionary<string, string>> LookupLabels { get; private set; } = new();
    private string searchQuery = string.Empty;

    public string SearchQuery
    {
        get => searchQuery;
        set => searchQuery = value;
    }

    public string? ErrorMessage { get; private set; }
    public bool IsLoading { get; private set; }
    public bool ShowDeleteModal { get; private set; }
    public long DeleteId { get; private set; }
    public string DeleteDisplayName { get; private set; } = string.Empty;
    public PaginationState Pagination { get; } = new();

    public async Task InitializeAsync(string? entityKey)
    {
        SelectedDefinition = MasterDataDefinitions.Find(entityKey);
        SearchQuery = string.Empty;
        Pagination.Reset();
        ErrorMessage = null;
        await LoadAsync();
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;
        await LoadLookupLabelsAsync();

        var result = await LoadItemsAsync(SelectedDefinition);
        if (result.IsSuccess && result.Data is not null)
        {
            Items = result.Data.Items.ToList();
            Pagination.ApplyMetadata(result.Data);
        }
        else
        {
            Items = new();
            ErrorMessage = result.Message;
            Pagination.SetTotal(0);
        }

        IsLoading = false;
    }

    public async Task LoadFirstPageAsync()
    {
        Pagination.Reset();
        await LoadAsync();
    }

    public IReadOnlyList<object> FilteredItems => Items;
    public IReadOnlyList<object> PagedItems => Items;

    public string GetDisplayValue(object item, MasterDataFieldDefinition field)
    {
        var value = GetRawValue(item, field.Name);
        if (value is null)
        {
            return "-";
        }

        if (!string.IsNullOrWhiteSpace(field.LookupKey) && LookupLabels.TryGetValue(field.LookupKey, out var labels) && labels.TryGetValue(value.ToString() ?? string.Empty, out var label))
        {
            return label;
        }

        return value switch
        {
            bool boolValue => boolValue ? "Active" : "Inactive",
            decimal decimalValue => decimalValue.ToString("0.##"),
            DateTime dateValue => dateValue.ToString("yyyy-MM-dd"),
            _ => value.ToString() ?? "-"
        };
    }

    public string GetPrimaryDisplayValue(object item)
        => GetRawValue(item, SelectedDefinition.PrimaryDisplayProperty)?.ToString() ?? SelectedDefinition.Title;

    public string GetBooleanBadgeClass(object item, MasterDataFieldDefinition field)
        => GetRawValue(item, field.Name) is bool value && value ? "tms-badge-success" : "bg-light text-secondary border";

    public long GetId(object item)
        => Convert.ToInt64(GetRawValue(item, SelectedDefinition.IdProperty));

    public void CreateNew()
        => navigationManager.NavigateTo($"master-data/{SelectedDefinition.Key}/new");

    public void Edit(object item)
        => navigationManager.NavigateTo($"master-data/{SelectedDefinition.Key}/{GetId(item)}/edit");

    public void ConfirmDelete(object item)
    {
        DeleteId = GetId(item);
        DeleteDisplayName = GetPrimaryDisplayValue(item);
        ShowDeleteModal = true;
    }

    public void CloseDeleteModal()
    {
        ShowDeleteModal = false;
    }

    public async Task ExecuteDeleteAsync()
    {
        ShowDeleteModal = false;
        var result = await masterDataService.DeleteAsync(SelectedDefinition.Endpoint, DeleteId);
        if (result.IsSuccess)
        {
            ErrorMessage = null;
            await LoadAsync();
        }
        else
        {
            ErrorMessage = result.Message;
        }
    }

    private async Task<OperationResult<PagedResult<object>>> LoadItemsAsync(MasterDataDefinition definition)
        => definition.Key switch
        {
            "profiles" => await LoadTypedItemsAsync<Profile>(definition.Endpoint, definition.Title),
            "factories" => await LoadTypedItemsAsync<Factory>(definition.Endpoint, definition.Title),
            "customers" => await LoadTypedItemsAsync<Customer>(definition.Endpoint, definition.Title),
            "locations" => await LoadTypedItemsAsync<Location>(definition.Endpoint, definition.Title),
            "carriers" => await LoadTypedItemsAsync<Carrier>(definition.Endpoint, definition.Title),
            "vehicles" => await LoadTypedItemsAsync<Vehicle>(definition.Endpoint, definition.Title),
            "drivers" => await LoadTypedItemsAsync<Driver>(definition.Endpoint, definition.Title),
            "vehicle-maintenance" => await LoadTypedItemsAsync<VehicleMaintenance>(definition.Endpoint, definition.Title),
            "fuel-transactions" => await LoadTypedItemsAsync<FuelTransaction>(definition.Endpoint, definition.Title),
            "driver-performance" => await LoadTypedItemsAsync<DriverPerformance>(definition.Endpoint, definition.Title),
            "safety-events" => await LoadTypedItemsAsync<TrackingEvent>(definition.Endpoint, definition.Title),
            "product-profiles" => await LoadTypedItemsAsync<ProductProfile>(definition.Endpoint, definition.Title),
            "product-groups" => await LoadTypedItemsAsync<ProductGroup>(definition.Endpoint, definition.Title),
            "product-categories" => await LoadTypedItemsAsync<ProductCategory>(definition.Endpoint, definition.Title),
            "units" => await LoadTypedItemsAsync<Unit>(definition.Endpoint, definition.Title),
            "products" => await LoadTypedItemsAsync<Product>(definition.Endpoint, definition.Title),
            "product-units" => await LoadTypedItemsAsync<ProductUnit>(definition.Endpoint, definition.Title),
            "provinces" => await LoadTypedItemsAsync<Province>(definition.Endpoint, definition.Title),
            "districts" => await LoadTypedItemsAsync<District>(definition.Endpoint, definition.Title),
            "subdistricts" => await LoadTypedItemsAsync<SubDistrict>(definition.Endpoint, definition.Title),
            _ => OperationResult<PagedResult<object>>.Failure("Unsupported master-data entity.")
        };

    private async Task<OperationResult<PagedResult<object>>> LoadTypedItemsAsync<T>(string endpoint, string label)
    {
        var result = await masterDataService.GetPagedListAsync<T>(endpoint, label, Pagination.CurrentPage, Pagination.PageSize, SearchQuery);
        if (!result.IsSuccess || result.Data is null)
        {
            return OperationResult<PagedResult<object>>.Failure(result.Message);
        }

        var page = PagedResult<object>.Create(result.Data.Items.Cast<object>().ToArray(), result.Data.TotalItems, result.Data.PageNumber, result.Data.PageSize);
        return OperationResult<PagedResult<object>>.Success(page, result.Message);
    }

    private async Task LoadLookupLabelsAsync()
    {
        var profileLabels = await BuildLookupAsync(masterDataService.GetProfilesAsync(), profile => profile.ProfileId, profile => $"{profile.ProfileName} ({profile.ProfileCode})");
        var labels = new Dictionary<string, Dictionary<string, string>>
        {
            ["profiles"] = profileLabels,
            ["factories"] = await BuildLookupAsync(masterDataService.GetFactoriesAsync(), factory => factory.FactoryId, factory => $"{factory.FactoryName} ({factory.FactoryCode})"),
            ["carriers"] = await BuildLookupAsync(masterDataService.GetCarriersAsync(), carrier => carrier.CarrierId, carrier => $"{carrier.CarrierName} ({carrier.CarrierCode})"),
            ["vehicles"] = await BuildLookupAsync(masterDataService.GetVehiclesAsync(), vehicle => vehicle.VehicleId, vehicle => vehicle.VehicleNo),
            ["drivers"] = await BuildLookupAsync(masterDataService.GetDriversAsync(), driver => driver.DriverId, driver => $"{driver.DriverName} ({driver.DriverCode})"),
            ["product-profiles"] = await BuildLookupAsync(masterDataService.GetProductProfilesAsync(), profile => profile.ProductProfileId, profile => $"{profile.ProductProfileName} ({profile.ProductProfileCode})"),
            ["product-groups"] = await BuildLookupAsync(masterDataService.GetProductGroupsAsync(), group => group.ProductGroupId, group => $"{group.ProductGroupName} ({group.ProductGroupCode})"),
            ["product-categories"] = await BuildLookupAsync(masterDataService.GetProductCategoriesAsync(), category => category.ProductCategoryId, category => $"{category.ProductCategoryName} ({category.ProductCategoryCode})"),
            ["units"] = await BuildLookupAsync(masterDataService.GetUnitsAsync(), unit => unit.UnitId, unit => $"{unit.UnitName} ({unit.UnitCode})"),
            ["products"] = await BuildLookupAsync(masterDataService.GetProductsAsync(), product => product.ProductId, product => $"{product.ProductName} ({product.ProductCode})"),
            ["provinces"] = await BuildLookupAsync(masterDataService.GetProvincesAsync(), province => province.ProvinceId, province => province.ProvinceNameTh),
            ["districts"] = await BuildLookupAsync(masterDataService.GetDistrictsAsync(), district => district.DistrictId, district => district.DistrictNameTh),
            ["subdistricts"] = await BuildLookupAsync(masterDataService.GetSubDistrictsAsync(), subDistrict => subDistrict.SubDistrictId, subDistrict => subDistrict.SubDistrictNameTh)
        };

        foreach (var pair in MasterDataDefinitions.ProfileLookupScopes)
        {
            labels[pair.Key] = profileLabels;
        }

        LookupLabels = labels;
    }

    private static async Task<Dictionary<string, string>> BuildLookupAsync<T>(Task<OperationResult<List<T>>> source, Func<T, long> idSelector, Func<T, string> labelSelector)
    {
        var result = await source;
        return result.Data?.ToDictionary(item => idSelector(item).ToString(), labelSelector) ?? new();
    }

    private static object? GetRawValue(object item, string propertyName)
        => item.GetType().GetProperty(propertyName)?.GetValue(item);
}

