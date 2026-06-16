using TmsSystem.Domain.Entities;

namespace TmsSystem.BlazorWasm.Models;

public enum MasterDataFieldType
{
    Text,
    Decimal,
    Checkbox,
    Select
}

public sealed record SelectOption(string Value, string Label);

public sealed record MasterDataFieldDefinition(
    string Name,
    string Label,
    MasterDataFieldType FieldType = MasterDataFieldType.Text,
    bool Required = false,
    string? LookupKey = null,
    IReadOnlyList<SelectOption>? StaticOptions = null);

public sealed record MasterDataDefinition(
    string Key,
    string Title,
    string Description,
    string IconCss,
    string Endpoint,
    Type EntityType,
    string IdProperty,
    string PrimaryDisplayProperty,
    IReadOnlyList<MasterDataFieldDefinition> ListFields,
    IReadOnlyList<MasterDataFieldDefinition> FormFields);

public static class MasterDataDefinitions
{
    public static IReadOnlyList<MasterDataDefinition> All { get; } =
    [
        new(
            "factories",
            "Factories",
            "Manufacturing sites and operating time zones",
            "bi bi-buildings",
            "api/factory",
            typeof(Factory),
            nameof(Factory.FactoryId),
            nameof(Factory.FactoryName),
            [
                Field(nameof(Factory.FactoryCode), "Code"),
                Field(nameof(Factory.FactoryName), "Factory"),
                Field(nameof(Factory.IndustryType), "Industry"),
                Field(nameof(Factory.TimeZone), "Time Zone"),
                Field(nameof(Factory.TaxId), "Tax ID"),
                Field(nameof(Factory.BranchCode), "Branch"),
                Field(nameof(Factory.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Factory.FactoryCode), "Factory Code", required: true),
                Field(nameof(Factory.FactoryName), "Factory Name", required: true),
                Field(nameof(Factory.IndustryType), "Industry Type", required: true),
                Field(nameof(Factory.TimeZone), "Time Zone"),
                Field(nameof(Factory.TaxId), "Tax ID"),
                Field(nameof(Factory.BranchCode), "Branch Code"),
                Field(nameof(Factory.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "customers",
            "Customers",
            "Customer accounts and contact information",
            "bi bi-person-badge",
            "api/customer",
            typeof(Customer),
            nameof(Customer.CustomerId),
            nameof(Customer.CustomerName),
            [
                Field(nameof(Customer.CustomerCode), "Code"),
                Field(nameof(Customer.CustomerName), "Customer"),
                Field(nameof(Customer.CustomerType), "Type"),
                Field(nameof(Customer.ContactName), "Contact"),
                Field(nameof(Customer.TaxId), "Tax ID"),
                Field(nameof(Customer.BranchCode), "Branch"),
                Field(nameof(Customer.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Customer.CustomerCode), "Customer Code", required: true),
                Field(nameof(Customer.CustomerName), "Customer Name", required: true),
                Field(nameof(Customer.CustomerType), "Customer Type"),
                Field(nameof(Customer.ContactName), "Contact Name"),
                Field(nameof(Customer.ContactEmail), "Contact Email"),
                Field(nameof(Customer.TaxId), "Tax ID"),
                Field(nameof(Customer.BranchCode), "Branch Code"),
                Field(nameof(Customer.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "locations",
            "Locations",
            "Pickup, delivery, warehouse, dock, and customer sites",
            "bi bi-geo-alt",
            "api/location",
            typeof(Location),
            nameof(Location.LocationId),
            nameof(Location.LocationName),
            [
                Field(nameof(Location.LocationCode), "Code"),
                Field(nameof(Location.LocationName), "Location"),
                Field(nameof(Location.LocationType), "Type"),
                Field(nameof(Location.FactoryId), "Factory", MasterDataFieldType.Select, lookupKey: "factories"),
                Field(nameof(Location.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Location.FactoryId), "Factory", MasterDataFieldType.Select, lookupKey: "factories"),
                Field(nameof(Location.LocationCode), "Location Code", required: true),
                Field(nameof(Location.LocationName), "Location Name", required: true),
                Field(nameof(Location.LocationType), "Location Type", MasterDataFieldType.Select, required: true, staticOptions: Options("Factory", "Warehouse", "Customer", "Port", "Cross Dock", "Other")),
                Field(nameof(Location.AddressText), "Address"),
                Field(nameof(Location.ProvinceId), "Province", MasterDataFieldType.Select, lookupKey: "provinces"),
                Field(nameof(Location.DistrictId), "District", MasterDataFieldType.Select, lookupKey: "districts"),
                Field(nameof(Location.SubDistrictId), "Sub-District", MasterDataFieldType.Select, lookupKey: "subdistricts"),
                Field(nameof(Location.Latitude), "Latitude", MasterDataFieldType.Decimal),
                Field(nameof(Location.Longitude), "Longitude", MasterDataFieldType.Decimal),
                Field(nameof(Location.DockCode), "Dock Code"),
                Field(nameof(Location.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "carriers",
            "Carriers",
            "Transport partners and integration readiness",
            "bi bi-truck",
            "api/carrier",
            typeof(Carrier),
            nameof(Carrier.CarrierId),
            nameof(Carrier.CarrierName),
            [
                Field(nameof(Carrier.CarrierCode), "Code"),
                Field(nameof(Carrier.CarrierName), "Carrier"),
                Field(nameof(Carrier.CarrierType), "Type"),
                Field(nameof(Carrier.SafetyRating), "Safety", MasterDataFieldType.Decimal),
                Field(nameof(Carrier.TaxId), "Tax ID"),
                Field(nameof(Carrier.BranchCode), "Branch"),
                Field(nameof(Carrier.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Carrier.CarrierCode), "Carrier Code", required: true),
                Field(nameof(Carrier.CarrierName), "Carrier Name", required: true),
                Field(nameof(Carrier.CarrierType), "Carrier Type", MasterDataFieldType.Select, staticOptions: Options("3PL", "Dedicated", "Spot", "Internal")),
                Field(nameof(Carrier.ApiEnabled), "API Enabled", MasterDataFieldType.Checkbox),
                Field(nameof(Carrier.EdiEnabled), "EDI Enabled", MasterDataFieldType.Checkbox),
                Field(nameof(Carrier.SafetyRating), "Safety Rating", MasterDataFieldType.Decimal),
                Field(nameof(Carrier.TaxId), "Tax ID"),
                Field(nameof(Carrier.BranchCode), "Branch Code"),
                Field(nameof(Carrier.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "vehicles",
            "Vehicles",
            "Fleet units, capacity, and temperature capability",
            "bi bi-car-front",
            "api/vehicle",
            typeof(Vehicle),
            nameof(Vehicle.VehicleId),
            nameof(Vehicle.VehicleNo),
            [
                Field(nameof(Vehicle.VehicleNo), "Vehicle No"),
                Field(nameof(Vehicle.VehicleType), "Type"),
                Field(nameof(Vehicle.CarrierId), "Carrier", MasterDataFieldType.Select, lookupKey: "carriers"),
                Field(nameof(Vehicle.TemperatureControlled), "Cold Chain", MasterDataFieldType.Checkbox),
                Field(nameof(Vehicle.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Vehicle.VehicleNo), "Vehicle No", required: true),
                Field(nameof(Vehicle.VehicleType), "Vehicle Type", MasterDataFieldType.Select, required: true, staticOptions: Options("4W", "6W", "10W", "Trailer", "Container", "Van")),
                Field(nameof(Vehicle.CarrierId), "Carrier", MasterDataFieldType.Select, lookupKey: "carriers"),
                Field(nameof(Vehicle.CapacityWeightKg), "Capacity Weight (kg)", MasterDataFieldType.Decimal),
                Field(nameof(Vehicle.CapacityVolumeM3), "Capacity Volume (m3)", MasterDataFieldType.Decimal),
                Field(nameof(Vehicle.TemperatureControlled), "Temperature Controlled", MasterDataFieldType.Checkbox),
                Field(nameof(Vehicle.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "drivers",
            "Drivers",
            "Driver profiles, carrier assignment, and licenses",
            "bi bi-person-vcard",
            "api/driver",
            typeof(Driver),
            nameof(Driver.DriverId),
            nameof(Driver.DriverName),
            [
                Field(nameof(Driver.DriverCode), "Code"),
                Field(nameof(Driver.DriverName), "Driver"),
                Field(nameof(Driver.CarrierId), "Carrier", MasterDataFieldType.Select, lookupKey: "carriers"),
                Field(nameof(Driver.MobileNo), "Mobile"),
                Field(nameof(Driver.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Driver.DriverCode), "Driver Code", required: true),
                Field(nameof(Driver.DriverName), "Driver Name", required: true),
                Field(nameof(Driver.CarrierId), "Carrier", MasterDataFieldType.Select, lookupKey: "carriers"),
                Field(nameof(Driver.MobileNo), "Mobile No"),
                Field(nameof(Driver.LicenseNo), "License No"),
                Field(nameof(Driver.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "products",
            "Products",
            "Goods, categories, hazard, and cold-chain rules",
            "bi bi-box-seam",
            "api/product",
            typeof(Product),
            nameof(Product.ProductId),
            nameof(Product.ProductName),
            [
                Field(nameof(Product.ProductCode), "Code"),
                Field(nameof(Product.ProductName), "Product"),
                Field(nameof(Product.ProductCategory), "Category"),
                Field(nameof(Product.ColdChainFlag), "Cold Chain", MasterDataFieldType.Checkbox),
                Field(nameof(Product.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Product.ProductCode), "Product Code", required: true),
                Field(nameof(Product.ProductName), "Product Name", required: true),
                Field(nameof(Product.ProductCategory), "Category"),
                Field(nameof(Product.HazardousFlag), "Hazardous", MasterDataFieldType.Checkbox),
                Field(nameof(Product.ColdChainFlag), "Cold Chain", MasterDataFieldType.Checkbox),
                Field(nameof(Product.MinTemperatureC), "Min Temperature (C)", MasterDataFieldType.Decimal),
                Field(nameof(Product.MaxTemperatureC), "Max Temperature (C)", MasterDataFieldType.Decimal),
                Field(nameof(Product.StackableFlag), "Stackable", MasterDataFieldType.Checkbox),
                Field(nameof(Product.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "provinces",
            "Provinces",
            "Province codes and local names",
            "bi bi-map",
            "api/province",
            typeof(Province),
            nameof(Province.ProvinceId),
            nameof(Province.ProvinceNameTh),
            [
                Field(nameof(Province.ProvinceCode), "Code"),
                Field(nameof(Province.ProvinceNameTh), "Thai Name"),
                Field(nameof(Province.ProvinceNameEn), "English Name"),
                Field(nameof(Province.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(Province.ProvinceCode), "Province Code", required: true),
                Field(nameof(Province.ProvinceNameTh), "Thai Name", required: true),
                Field(nameof(Province.ProvinceNameEn), "English Name"),
                Field(nameof(Province.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "districts",
            "Districts",
            "District setup under provinces",
            "bi bi-map-fill",
            "api/district",
            typeof(District),
            nameof(District.DistrictId),
            nameof(District.DistrictNameTh),
            [
                Field(nameof(District.DistrictCode), "Code"),
                Field(nameof(District.DistrictNameTh), "Thai Name"),
                Field(nameof(District.ProvinceId), "Province", MasterDataFieldType.Select, lookupKey: "provinces"),
                Field(nameof(District.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(District.ProvinceId), "Province", MasterDataFieldType.Select, required: true, lookupKey: "provinces"),
                Field(nameof(District.DistrictCode), "District Code", required: true),
                Field(nameof(District.DistrictNameTh), "Thai Name", required: true),
                Field(nameof(District.DistrictNameEn), "English Name"),
                Field(nameof(District.IsActive), "Active", MasterDataFieldType.Checkbox)
            ]),
        new(
            "subdistricts",
            "Sub-Districts",
            "Sub-district and postal-code setup",
            "bi bi-signpost",
            "api/subdistrict",
            typeof(SubDistrict),
            nameof(SubDistrict.SubDistrictId),
            nameof(SubDistrict.SubDistrictNameTh),
            [
                Field(nameof(SubDistrict.SubDistrictCode), "Code"),
                Field(nameof(SubDistrict.SubDistrictNameTh), "Thai Name"),
                Field(nameof(SubDistrict.DistrictId), "District", MasterDataFieldType.Select, lookupKey: "districts"),
                Field(nameof(SubDistrict.PostalCode), "Postal"),
                Field(nameof(SubDistrict.IsActive), "Active", MasterDataFieldType.Checkbox)
            ],
            [
                Field(nameof(SubDistrict.DistrictId), "District", MasterDataFieldType.Select, required: true, lookupKey: "districts"),
                Field(nameof(SubDistrict.SubDistrictCode), "Sub-District Code", required: true),
                Field(nameof(SubDistrict.SubDistrictNameTh), "Thai Name", required: true),
                Field(nameof(SubDistrict.SubDistrictNameEn), "English Name"),
                Field(nameof(SubDistrict.PostalCode), "Postal Code"),
                Field(nameof(SubDistrict.IsActive), "Active", MasterDataFieldType.Checkbox)
            ])
    ];

    public static MasterDataDefinition Default => All[0];

    public static MasterDataDefinition Find(string? key)
        => All.FirstOrDefault(definition => string.Equals(definition.Key, key, StringComparison.OrdinalIgnoreCase)) ?? Default;

    private static MasterDataFieldDefinition Field(
        string name,
        string label,
        MasterDataFieldType fieldType = MasterDataFieldType.Text,
        bool required = false,
        string? lookupKey = null,
        IReadOnlyList<SelectOption>? staticOptions = null)
        => new(name, label, fieldType, required, lookupKey, staticOptions);

    private static IReadOnlyList<SelectOption> Options(params string[] values)
        => values.Select(value => new SelectOption(value, value)).ToArray();
}
