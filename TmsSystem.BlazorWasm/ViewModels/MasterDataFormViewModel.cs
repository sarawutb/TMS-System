using System.Globalization;
using Microsoft.AspNetCore.Components;
using TmsSystem.Application.Common;
using TmsSystem.BlazorWasm.Models;
using TmsSystem.BlazorWasm.Services;
using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class MasterDataFormViewModel(MasterDataService masterDataService, NavigationManager navigationManager)
{
    private readonly Dictionary<string, List<SelectOption>> lookupOptions = new();

    public IReadOnlyList<MasterDataDefinition> Definitions => MasterDataDefinitions.All;
    public MasterDataDefinition SelectedDefinition { get; private set; } = MasterDataDefinitions.Default;
    public object Record { get; private set; } = new Factory();
    public bool IsLoading { get; private set; }
    public bool IsSaving { get; private set; }
    public string? ErrorMessage { get; private set; }
    public IReadOnlyDictionary<string, List<SelectOption>> LookupOptions => lookupOptions;

    public async Task InitializeAsync(string? entityKey, long? id)
    {
        IsLoading = true;
        IsSaving = false;
        ErrorMessage = null;
        SelectedDefinition = MasterDataDefinitions.Find(entityKey);
        await LoadLookupsAsync();

        if (id.HasValue)
        {
            var result = await LoadRecordAsync(SelectedDefinition, id.Value);
            if (result.IsSuccess && result.Data is not null)
            {
                Record = result.Data;
            }
            else
            {
                Record = Activator.CreateInstance(SelectedDefinition.EntityType) ?? new Factory();
                ErrorMessage = result.Message;
            }
        }
        else
        {
            Record = Activator.CreateInstance(SelectedDefinition.EntityType) ?? new Factory();
            SetRawValue(nameof(Factory.IsActive), true);
        }

        await RefreshDependentLookupsAsync();
        IsLoading = false;
    }

    public IReadOnlyList<SelectOption> GetOptions(MasterDataFieldDefinition field)
    {
        if (field.StaticOptions is not null)
        {
            return field.StaticOptions;
        }

        return field.LookupKey is not null && lookupOptions.TryGetValue(field.LookupKey, out var options)
            ? options
            : Array.Empty<SelectOption>();
    }

    public string GetFieldText(MasterDataFieldDefinition field)
    {
        var value = GetRawValue(field.Name);
        return value switch
        {
            null => string.Empty,
            DateTime dateValue => dateValue == default ? string.Empty : dateValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            decimal decimalValue => decimalValue.ToString("0.##", CultureInfo.InvariantCulture),
            bool boolValue => boolValue ? "true" : "false",
            _ => value.ToString() ?? string.Empty
        };
    }

    public bool GetBoolValue(MasterDataFieldDefinition field)
        => GetRawValue(field.Name) is bool value && value;

    public async Task SetFieldValueAsync(MasterDataFieldDefinition field, string? value)
    {
        var property = Record.GetType().GetProperty(field.Name);
        if (property is null)
        {
            return;
        }

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        object? convertedValue = null;
        if (!string.IsNullOrWhiteSpace(value))
        {
            convertedValue = targetType == typeof(string)
                ? value
                : targetType == typeof(DateTime)
                    ? DateTime.Parse(value, CultureInfo.InvariantCulture)
                    : targetType == typeof(decimal)
                        ? decimal.Parse(value, CultureInfo.InvariantCulture)
                        : targetType == typeof(long)
                            ? long.Parse(value, CultureInfo.InvariantCulture)
                            : targetType == typeof(bool)
                                ? bool.Parse(value)
                                : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        else if (targetType == typeof(string))
        {
            convertedValue = string.Empty;
        }

        property.SetValue(Record, convertedValue);
        await HandleDependentFieldChangeAsync(field.Name);
    }

    public async Task SetCheckboxValueAsync(MasterDataFieldDefinition field, bool value)
    {
        SetRawValue(field.Name, value);
        await HandleDependentFieldChangeAsync(field.Name);
    }

    public async Task SaveAsync(long? id)
    {
        DefaultBranchCode();
        ErrorMessage = Validate();
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            return;
        }

        IsSaving = true;
        OperationResult<object> result = id.HasValue
            ? await UpdateRecordAsync(SelectedDefinition, id.Value)
            : await CreateRecordAsync(SelectedDefinition);

        IsSaving = false;
        if (result.IsSuccess)
        {
            navigationManager.NavigateTo($"master-data/{SelectedDefinition.Key}");
            return;
        }

        ErrorMessage = result.Message;
    }

    public void Cancel()
        => navigationManager.NavigateTo($"master-data/{SelectedDefinition.Key}");

    private string? Validate()
    {
        foreach (var field in SelectedDefinition.FormFields.Where(field => field.Required))
        {
            var value = GetRawValue(field.Name);
            if (value is null || string.IsNullOrWhiteSpace(value.ToString()) || value is long longValue && longValue == 0 || value is DateTime dateValue && dateValue == default)
            {
                return $"{field.Label} is required.";
            }
        }

        return null;
    }

    private void DefaultBranchCode()
    {
        if (SelectedDefinition.FormFields.Any(field => field.Name == nameof(Factory.BranchCode)) &&
            string.IsNullOrWhiteSpace(GetRawValue(nameof(Factory.BranchCode))?.ToString()))
        {
            SetRawValue(nameof(Factory.BranchCode), "00000");
        }
    }

    private async Task LoadLookupsAsync()
    {
        lookupOptions["factories"] = await BuildOptionsAsync(masterDataService.GetFactoriesAsync(), factory => factory.FactoryId, factory => $"{factory.FactoryName} ({factory.FactoryCode})");
        lookupOptions["carriers"] = await BuildOptionsAsync(masterDataService.GetCarriersAsync(), carrier => carrier.CarrierId, carrier => $"{carrier.CarrierName} ({carrier.CarrierCode})");
        lookupOptions["vehicles"] = await BuildOptionsAsync(masterDataService.GetVehiclesAsync(), vehicle => vehicle.VehicleId, vehicle => vehicle.VehicleNo);
        lookupOptions["drivers"] = await BuildOptionsAsync(masterDataService.GetDriversAsync(), driver => driver.DriverId, driver => $"{driver.DriverName} ({driver.DriverCode})");
        lookupOptions["product-profiles"] = await BuildOptionsAsync(masterDataService.GetProductProfilesAsync(), profile => profile.ProductProfileId, profile => $"{profile.ProductProfileName} ({profile.ProductProfileCode})");
        lookupOptions["product-groups"] = await BuildOptionsAsync(masterDataService.GetProductGroupsAsync(), group => group.ProductGroupId, group => $"{group.ProductGroupName} ({group.ProductGroupCode})");
        lookupOptions["product-categories"] = await BuildOptionsAsync(masterDataService.GetProductCategoriesAsync(), category => category.ProductCategoryId, category => $"{category.ProductCategoryName} ({category.ProductCategoryCode})");
        lookupOptions["units"] = await BuildOptionsAsync(masterDataService.GetUnitsAsync(), unit => unit.UnitId, unit => $"{unit.UnitName} ({unit.UnitCode})");
        lookupOptions["products"] = await BuildOptionsAsync(masterDataService.GetProductsAsync(), product => product.ProductId, product => $"{product.ProductName} ({product.ProductCode})");
        lookupOptions["provinces"] = await BuildOptionsAsync(masterDataService.GetProvincesAsync(), province => province.ProvinceId, province => province.ProvinceNameTh);
        lookupOptions["districts"] = await BuildOptionsAsync(masterDataService.GetDistrictsAsync(), district => district.DistrictId, district => district.DistrictNameTh);
        lookupOptions["subdistricts"] = await BuildOptionsAsync(masterDataService.GetSubDistrictsAsync(), subDistrict => subDistrict.SubDistrictId, subDistrict => subDistrict.SubDistrictNameTh);
    }

    private async Task RefreshDependentLookupsAsync()
    {
        if (SelectedDefinition.Key is not ("locations" or "customers"))
        {
            return;
        }

        var provinceId = GetRawValue(nameof(Location.ProvinceId)) as long?;
        var districtId = GetRawValue(nameof(Location.DistrictId)) as long?;

        if (provinceId.HasValue)
        {
            lookupOptions["districts"] = await BuildOptionsAsync(masterDataService.GetDistrictsByProvinceAsync(provinceId.Value), district => district.DistrictId, district => district.DistrictNameTh);
        }

        if (districtId.HasValue)
        {
            lookupOptions["subdistricts"] = await BuildOptionsAsync(masterDataService.GetSubDistrictsByDistrictAsync(districtId.Value), subDistrict => subDistrict.SubDistrictId, subDistrict => subDistrict.SubDistrictNameTh);
        }
    }

    private async Task HandleDependentFieldChangeAsync(string fieldName)
    {
        if (SelectedDefinition.Key is not ("locations" or "customers"))
        {
            return;
        }

        if (fieldName == nameof(Location.ProvinceId))
        {
            SetRawValue(nameof(Location.DistrictId), null);
            SetRawValue(nameof(Location.SubDistrictId), null);
            var provinceId = GetRawValue(nameof(Location.ProvinceId)) as long?;
            lookupOptions["districts"] = provinceId.HasValue
                ? await BuildOptionsAsync(masterDataService.GetDistrictsByProvinceAsync(provinceId.Value), district => district.DistrictId, district => district.DistrictNameTh)
                : new();
            lookupOptions["subdistricts"] = new();
        }

        if (fieldName == nameof(Location.DistrictId))
        {
            SetRawValue(nameof(Location.SubDistrictId), null);
            var districtId = GetRawValue(nameof(Location.DistrictId)) as long?;
            lookupOptions["subdistricts"] = districtId.HasValue
                ? await BuildOptionsAsync(masterDataService.GetSubDistrictsByDistrictAsync(districtId.Value), subDistrict => subDistrict.SubDistrictId, subDistrict => subDistrict.SubDistrictNameTh)
                : new();
        }
    }

    private async Task<OperationResult<object>> LoadRecordAsync(MasterDataDefinition definition, long id)
        => definition.Key switch
        {
            "factories" => await LoadTypedRecordAsync<Factory>(definition.Endpoint, id),
            "customers" => await LoadTypedRecordAsync<Customer>(definition.Endpoint, id),
            "locations" => await LoadTypedRecordAsync<Location>(definition.Endpoint, id),
            "carriers" => await LoadTypedRecordAsync<Carrier>(definition.Endpoint, id),
            "vehicles" => await LoadTypedRecordAsync<Vehicle>(definition.Endpoint, id),
            "drivers" => await LoadTypedRecordAsync<Driver>(definition.Endpoint, id),
            "vehicle-maintenance" => await LoadTypedRecordAsync<VehicleMaintenance>(definition.Endpoint, id),
            "fuel-transactions" => await LoadTypedRecordAsync<FuelTransaction>(definition.Endpoint, id),
            "driver-performance" => await LoadTypedRecordAsync<DriverPerformance>(definition.Endpoint, id),
            "safety-events" => await LoadTypedRecordAsync<TrackingEvent>(definition.Endpoint, id),
            "product-profiles" => await LoadTypedRecordAsync<ProductProfile>(definition.Endpoint, id),
            "product-groups" => await LoadTypedRecordAsync<ProductGroup>(definition.Endpoint, id),
            "product-categories" => await LoadTypedRecordAsync<ProductCategory>(definition.Endpoint, id),
            "units" => await LoadTypedRecordAsync<Unit>(definition.Endpoint, id),
            "products" => await LoadTypedRecordAsync<Product>(definition.Endpoint, id),
            "product-units" => await LoadTypedRecordAsync<ProductUnit>(definition.Endpoint, id),
            "provinces" => await LoadTypedRecordAsync<Province>(definition.Endpoint, id),
            "districts" => await LoadTypedRecordAsync<District>(definition.Endpoint, id),
            "subdistricts" => await LoadTypedRecordAsync<SubDistrict>(definition.Endpoint, id),
            _ => OperationResult<object>.Failure("Unsupported master-data entity.")
        };

    private async Task<OperationResult<object>> CreateRecordAsync(MasterDataDefinition definition)
        => definition.Key switch
        {
            "factories" => await CreateTypedRecordAsync<Factory>(definition.Endpoint),
            "customers" => await CreateTypedRecordAsync<Customer>(definition.Endpoint),
            "locations" => await CreateTypedRecordAsync<Location>(definition.Endpoint),
            "carriers" => await CreateTypedRecordAsync<Carrier>(definition.Endpoint),
            "vehicles" => await CreateTypedRecordAsync<Vehicle>(definition.Endpoint),
            "drivers" => await CreateTypedRecordAsync<Driver>(definition.Endpoint),
            "vehicle-maintenance" => await CreateTypedRecordAsync<VehicleMaintenance>(definition.Endpoint),
            "fuel-transactions" => await CreateTypedRecordAsync<FuelTransaction>(definition.Endpoint),
            "driver-performance" => await CreateTypedRecordAsync<DriverPerformance>(definition.Endpoint),
            "safety-events" => await CreateTypedRecordAsync<TrackingEvent>(definition.Endpoint),
            "product-profiles" => await CreateTypedRecordAsync<ProductProfile>(definition.Endpoint),
            "product-groups" => await CreateTypedRecordAsync<ProductGroup>(definition.Endpoint),
            "product-categories" => await CreateTypedRecordAsync<ProductCategory>(definition.Endpoint),
            "units" => await CreateTypedRecordAsync<Unit>(definition.Endpoint),
            "products" => await CreateTypedRecordAsync<Product>(definition.Endpoint),
            "product-units" => await CreateTypedRecordAsync<ProductUnit>(definition.Endpoint),
            "provinces" => await CreateTypedRecordAsync<Province>(definition.Endpoint),
            "districts" => await CreateTypedRecordAsync<District>(definition.Endpoint),
            "subdistricts" => await CreateTypedRecordAsync<SubDistrict>(definition.Endpoint),
            _ => OperationResult<object>.Failure("Unsupported master-data entity.")
        };

    private async Task<OperationResult<object>> UpdateRecordAsync(MasterDataDefinition definition, long id)
        => definition.Key switch
        {
            "factories" => await UpdateTypedRecordAsync<Factory>(definition.Endpoint, id),
            "customers" => await UpdateTypedRecordAsync<Customer>(definition.Endpoint, id),
            "locations" => await UpdateTypedRecordAsync<Location>(definition.Endpoint, id),
            "carriers" => await UpdateTypedRecordAsync<Carrier>(definition.Endpoint, id),
            "vehicles" => await UpdateTypedRecordAsync<Vehicle>(definition.Endpoint, id),
            "drivers" => await UpdateTypedRecordAsync<Driver>(definition.Endpoint, id),
            "vehicle-maintenance" => await UpdateTypedRecordAsync<VehicleMaintenance>(definition.Endpoint, id),
            "fuel-transactions" => await UpdateTypedRecordAsync<FuelTransaction>(definition.Endpoint, id),
            "driver-performance" => await UpdateTypedRecordAsync<DriverPerformance>(definition.Endpoint, id),
            "safety-events" => await UpdateTypedRecordAsync<TrackingEvent>(definition.Endpoint, id),
            "product-profiles" => await UpdateTypedRecordAsync<ProductProfile>(definition.Endpoint, id),
            "product-groups" => await UpdateTypedRecordAsync<ProductGroup>(definition.Endpoint, id),
            "product-categories" => await UpdateTypedRecordAsync<ProductCategory>(definition.Endpoint, id),
            "units" => await UpdateTypedRecordAsync<Unit>(definition.Endpoint, id),
            "products" => await UpdateTypedRecordAsync<Product>(definition.Endpoint, id),
            "product-units" => await UpdateTypedRecordAsync<ProductUnit>(definition.Endpoint, id),
            "provinces" => await UpdateTypedRecordAsync<Province>(definition.Endpoint, id),
            "districts" => await UpdateTypedRecordAsync<District>(definition.Endpoint, id),
            "subdistricts" => await UpdateTypedRecordAsync<SubDistrict>(definition.Endpoint, id),
            _ => OperationResult<object>.Failure("Unsupported master-data entity.")
        };

    private async Task<OperationResult<object>> LoadTypedRecordAsync<T>(string endpoint, long id)
    {
        var result = await masterDataService.GetByIdAsync<T>(endpoint, id);
        return result.IsSuccess && result.Data is not null
            ? OperationResult<object>.Success(result.Data, result.Message)
            : OperationResult<object>.Failure(result.Message);
    }

    private async Task<OperationResult<object>> CreateTypedRecordAsync<T>(string endpoint)
    {
        var result = await masterDataService.CreateAsync(endpoint, (T)Record);
        return result.IsSuccess && result.Data is not null
            ? OperationResult<object>.Success(result.Data, result.Message)
            : OperationResult<object>.Failure(result.Message);
    }

    private async Task<OperationResult<object>> UpdateTypedRecordAsync<T>(string endpoint, long id)
    {
        var result = await masterDataService.UpdateAsync(endpoint, id, (T)Record);
        return result.IsSuccess && result.Data is not null
            ? OperationResult<object>.Success(result.Data, result.Message)
            : OperationResult<object>.Failure(result.Message);
    }

    private static async Task<List<SelectOption>> BuildOptionsAsync<T>(Task<OperationResult<List<T>>> source, Func<T, long> idSelector, Func<T, string> labelSelector)
    {
        var result = await source;
        return result.Data?
            .OrderBy(labelSelector)
            .Select(item => new SelectOption(idSelector(item).ToString(CultureInfo.InvariantCulture), labelSelector(item)))
            .ToList() ?? new();
    }

    private object? GetRawValue(string propertyName)
        => Record.GetType().GetProperty(propertyName)?.GetValue(Record);

    private void SetRawValue(string propertyName, object? value)
    {
        var property = Record.GetType().GetProperty(propertyName);
        property?.SetValue(Record, value);
    }
}


