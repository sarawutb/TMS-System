using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Common;
using TmsSystem.Domain.Entities;

namespace TmsSystem.Infrastructure.Data;

public sealed class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Factory> Factories => Set<Factory>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<SubDistrict> SubDistricts => Set<SubDistrict>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Driver> Drivers => Set<Driver>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<IntegrationPartner> IntegrationPartners => Set<IntegrationPartner>();
    public DbSet<IntegrationMessage> IntegrationMessages => Set<IntegrationMessage>();
    public DbSet<TransportOrder> TransportOrders => Set<TransportOrder>();
    public DbSet<TransportOrderItem> TransportOrderItems => Set<TransportOrderItem>();
    public DbSet<RoutePlan> RoutePlans => Set<RoutePlan>();
    public DbSet<LoadPlan> LoadPlans => Set<LoadPlan>();
    public DbSet<CarrierTender> CarrierTenders => Set<CarrierTender>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentStop> ShipmentStops => Set<ShipmentStop>();
    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();
    public DbSet<Epod> Epods => Set<Epod>();
    public DbSet<ShipmentException> Exceptions => Set<ShipmentException>();
    public DbSet<VehicleMaintenance> VehicleMaintenances => Set<VehicleMaintenance>();
    public DbSet<FuelTransaction> FuelTransactions => Set<FuelTransaction>();
    public DbSet<DriverPerformance> DriverPerformances => Set<DriverPerformance>();
    public DbSet<FreightContract> FreightContracts => Set<FreightContract>();
    public DbSet<FreightAudit> FreightAudits => Set<FreightAudit>();
    public DbSet<BillingInvoice> BillingInvoices => Set<BillingInvoice>();
    public DbSet<IotDevice> IotDevices => Set<IotDevice>();
    public DbSet<IotSensorReading> IotSensorReadings => Set<IotSensorReading>();
    public DbSet<EtaPrediction> EtaPredictions => Set<EtaPrediction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureAudit(modelBuilder);
        ConfigureSecurity(modelBuilder);

        modelBuilder.Entity<Factory>(entity =>
        {
            entity.ToTable("mst_factory");
            entity.HasKey(x => x.FactoryId);
            entity.Property(x => x.FactoryCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.FactoryNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.FactoryNameEn).HasMaxLength(200);
            entity.Property(x => x.FactoryNameShort).HasMaxLength(50);
            entity.Property(x => x.IndustryType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TimeZone).HasMaxLength(100);
            entity.Property(x => x.TaxId).HasMaxLength(13);
            entity.Property(x => x.BranchCode).HasMaxLength(5).HasDefaultValue("00000").IsRequired(false);
            entity.HasIndex(x => x.FactoryCode).HasDatabaseName("UX_mst_factory_FactoryCode").IsUnique();
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("mst_customer");
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CustomerNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CustomerNameEn).HasMaxLength(200);
            entity.Property(x => x.CustomerNameShort).HasMaxLength(50);
            entity.Property(x => x.CustomerType).HasMaxLength(50);
            entity.Property(x => x.ContactName).HasMaxLength(150);
            entity.Property(x => x.ContactEmail).HasMaxLength(150);
            entity.Property(x => x.TaxId).HasMaxLength(13);
            entity.Property(x => x.BranchCode).HasMaxLength(5).HasDefaultValue("00000").IsRequired(false);
            entity.Property(x => x.AddressText).HasMaxLength(500);
            entity.HasOne(x => x.Province).WithMany().HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.District).WithMany().HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubDistrict).WithMany().HasForeignKey(x => x.SubDistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.CustomerCode).HasDatabaseName("UX_mst_customer_CustomerCode").IsUnique();
        });

        modelBuilder.Entity<Province>(entity =>
        {
            entity.ToTable("mst_province");
            entity.HasKey(x => x.ProvinceId);
            entity.Property(x => x.ProvinceCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ProvinceNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProvinceNameEn).HasMaxLength(200);
            entity.HasIndex(x => x.ProvinceCode).HasDatabaseName("UX_mst_province_ProvinceCode").IsUnique();
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("mst_district");
            entity.HasKey(x => x.DistrictId);
            entity.Property(x => x.DistrictCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DistrictNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.DistrictNameEn).HasMaxLength(200);
            entity.HasOne(x => x.Province).WithMany(x => x.Districts).HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.DistrictCode).HasDatabaseName("UX_mst_district_DistrictCode").IsUnique();
        });

        modelBuilder.Entity<SubDistrict>(entity =>
        {
            entity.ToTable("mst_subdistrict");
            entity.HasKey(x => x.SubDistrictId);
            entity.Property(x => x.SubDistrictCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SubDistrictNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SubDistrictNameEn).HasMaxLength(200);
            entity.Property(x => x.PostalCode).HasMaxLength(20);
            entity.HasOne(x => x.District).WithMany(x => x.SubDistricts).HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.SubDistrictCode).HasDatabaseName("UX_mst_subdistrict_SubDistrictCode").IsUnique();
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("mst_location");
            entity.HasKey(x => x.LocationId);
            entity.Property(x => x.LocationCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LocationNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.LocationNameEn).HasMaxLength(200);
            entity.Property(x => x.LocationNameShort).HasMaxLength(50);
            entity.Property(x => x.LocationType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AddressText).HasMaxLength(500);
            entity.Property(x => x.Latitude).HasPrecision(10, 7);
            entity.Property(x => x.Longitude).HasPrecision(10, 7);
            entity.Property(x => x.DockCode).HasMaxLength(50);
            entity.HasOne(x => x.Factory).WithMany(x => x.Locations).HasForeignKey(x => x.FactoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Province).WithMany(x => x.Locations).HasForeignKey(x => x.ProvinceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.District).WithMany(x => x.Locations).HasForeignKey(x => x.DistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.SubDistrict).WithMany(x => x.Locations).HasForeignKey(x => x.SubDistrictId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.LocationCode).HasDatabaseName("UX_mst_location_LocationCode").IsUnique();
        });

        modelBuilder.Entity<Carrier>(entity =>
        {
            entity.ToTable("mst_carrier");
            entity.HasKey(x => x.CarrierId);
            entity.Property(x => x.CarrierCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CarrierNameTh).HasMaxLength(200).IsRequired();
            entity.Property(x => x.CarrierNameEn).HasMaxLength(200);
            entity.Property(x => x.CarrierNameShort).HasMaxLength(50);
            entity.Property(x => x.CarrierType).HasMaxLength(50);
            entity.Property(x => x.SafetyRating).HasPrecision(5, 2);
            entity.Property(x => x.TaxId).HasMaxLength(13);
            entity.Property(x => x.BranchCode).HasMaxLength(5).HasDefaultValue("00000").IsRequired(false);
            entity.HasIndex(x => x.CarrierCode).HasDatabaseName("UX_mst_carrier_CarrierCode").IsUnique();
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("mst_vehicle");
            entity.HasKey(x => x.VehicleId);
            entity.Property(x => x.VehicleNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VehicleType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CapacityWeightKg).HasPrecision(18, 2);
            entity.Property(x => x.CapacityVolumeM3).HasPrecision(18, 2);
            entity.HasOne(x => x.Carrier).WithMany(x => x.Vehicles).HasForeignKey(x => x.CarrierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.VehicleNo).HasDatabaseName("UX_mst_vehicle_VehicleNo").IsUnique();
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("mst_driver");
            entity.HasKey(x => x.DriverId);
            entity.Property(x => x.DriverCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DriverName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MobileNo).HasMaxLength(50);
            entity.Property(x => x.LicenseNo).HasMaxLength(100);
            entity.HasOne(x => x.Carrier).WithMany(x => x.Drivers).HasForeignKey(x => x.CarrierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.DriverCode).HasDatabaseName("UX_mst_driver_DriverCode").IsUnique();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("mst_product");
            entity.HasKey(x => x.ProductId);
            entity.Property(x => x.ProductCode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ProductName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ProductCategory).HasMaxLength(80);
            entity.Property(x => x.MinTemperatureC).HasPrecision(8, 2);
            entity.Property(x => x.MaxTemperatureC).HasPrecision(8, 2);
            entity.HasIndex(x => x.ProductCode).HasDatabaseName("UX_mst_product_ProductCode").IsUnique();
        });

        modelBuilder.Entity<IntegrationPartner>(entity =>
        {
            entity.ToTable("cfg_integration_partner");
            entity.HasKey(x => x.IntegrationPartnerId);
            entity.Property(x => x.PartnerCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PartnerName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.SystemType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.IntegrationMethod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EndpointUrl).HasMaxLength(500);
            entity.HasIndex(x => x.PartnerCode).HasDatabaseName("UX_cfg_integration_partner_PartnerCode").IsUnique();
        });

        modelBuilder.Entity<IntegrationMessage>(entity =>
        {
            entity.ToTable("trn_integration_message");
            entity.HasKey(x => x.IntegrationMessageId);
            entity.Property(x => x.Direction).HasMaxLength(20).IsRequired();
            entity.Property(x => x.MessageType).HasMaxLength(80).IsRequired();
            entity.Property(x => x.ReferenceNo).HasMaxLength(100);
            entity.Property(x => x.PayloadRef).HasMaxLength(500);
            entity.Property(x => x.MessageStatus).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<TransportOrder>(entity =>
        {
            entity.ToTable("trn_transport_order");
            entity.HasKey(x => x.TransportOrderId);
            entity.Property(x => x.TransportOrderNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SourceSystem).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SourceDocumentNo).HasMaxLength(100);
            entity.Property(x => x.Priority).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.TransportOrderNo).HasDatabaseName("UX_trn_transport_order_TransportOrderNo").IsUnique();
        });

        modelBuilder.Entity<TransportOrderItem>(entity =>
        {
            entity.ToTable("trn_transport_order_item");
            entity.HasKey(x => x.TransportOrderItemId);
            entity.Property(x => x.ProductCode).HasMaxLength(80);
            entity.Property(x => x.ProductDescription).HasMaxLength(300);
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.Property(x => x.UnitName).HasMaxLength(30).IsRequired();
            entity.Property(x => x.WeightKg).HasPrecision(18, 3);
            entity.Property(x => x.VolumeM3).HasPrecision(18, 3);
        });

        modelBuilder.Entity<RoutePlan>(entity =>
        {
            entity.ToTable("trn_route_plan");
            entity.HasKey(x => x.RoutePlanId);
            entity.Property(x => x.RoutePlanNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OptimizationEngine).HasMaxLength(100);
            entity.Property(x => x.TransportMode).HasMaxLength(50);
            entity.Property(x => x.VehicleType).HasMaxLength(50);
            entity.Property(x => x.TotalDistanceKm).HasPrecision(18, 2);
            entity.Property(x => x.EstimatedCost).HasPrecision(18, 2);
            entity.Property(x => x.RiskScore).HasPrecision(5, 2);
            entity.Property(x => x.StopSequenceJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ComplianceIssuesJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.SolverMetadataJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Carrier).WithMany().HasForeignKey(x => x.CarrierId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.RoutePlanNo).HasDatabaseName("UX_trn_route_plan_RoutePlanNo").IsUnique();
        });

        modelBuilder.Entity<LoadPlan>(entity =>
        {
            entity.ToTable("trn_load_plan");
            entity.HasKey(x => x.LoadPlanId);
            entity.Property(x => x.LoadPlanNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.VehicleType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ContainerLengthM).HasPrecision(18, 3);
            entity.Property(x => x.ContainerWidthM).HasPrecision(18, 3);
            entity.Property(x => x.ContainerHeightM).HasPrecision(18, 3);
            entity.Property(x => x.CapacityWeightKg).HasPrecision(18, 2);
            entity.Property(x => x.CapacityVolumeM3).HasPrecision(18, 2);
            entity.Property(x => x.TotalWeightKg).HasPrecision(18, 2);
            entity.Property(x => x.TotalVolumeM3).HasPrecision(18, 2);
            entity.Property(x => x.UtilizationPercent).HasPrecision(5, 2);
            entity.Property(x => x.ThreeDPlanRef).HasMaxLength(500);
            entity.Property(x => x.LoadPlanJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.PlacementJson).HasColumnType("nvarchar(max)");
            entity.Property(x => x.ConstraintIssuesJson).HasColumnType("nvarchar(max)");
            entity.HasOne(x => x.RoutePlan).WithMany(x => x.LoadPlans).HasForeignKey(x => x.RoutePlanId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.LoadPlanNo).HasDatabaseName("UX_trn_load_plan_LoadPlanNo").IsUnique();
        });

        modelBuilder.Entity<CarrierTender>(entity =>
        {
            entity.ToTable("trn_carrier_tender");
            entity.HasKey(x => x.CarrierTenderId);
            entity.Property(x => x.TenderNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.TenderStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.OfferedCost).HasPrecision(18, 2);
            entity.Property(x => x.RejectReason).HasMaxLength(500);
            entity.HasIndex(x => x.TenderNo).HasDatabaseName("UX_trn_carrier_tender_TenderNo").IsUnique();
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.ToTable("trn_shipment");
            entity.HasKey(x => x.ShipmentId);
            entity.Property(x => x.ShipmentNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ShipmentStatus).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.ShipmentNo).HasDatabaseName("UX_trn_shipment_ShipmentNo").IsUnique();
        });

        modelBuilder.Entity<ShipmentStop>(entity =>
        {
            entity.ToTable("trn_shipment_stop");
            entity.HasKey(x => x.ShipmentStopId);
            entity.Property(x => x.StopType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.DockCode).HasMaxLength(50);
            entity.Property(x => x.StopStatus).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<TrackingEvent>(entity =>
        {
            entity.ToTable("trn_tracking_event");
            entity.HasKey(x => x.TrackingEventId);
            entity.Property(x => x.EventCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.EventName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Latitude).HasPrecision(10, 7);
            entity.Property(x => x.Longitude).HasPrecision(10, 7);
            entity.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.SafetyEventType).HasMaxLength(50);
            entity.Property(x => x.ExternalEventRef).HasMaxLength(100);
            entity.Property(x => x.Remark).HasMaxLength(500);
            entity.HasOne<Shipment>().WithMany().HasForeignKey(x => x.ShipmentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Driver>().WithMany().HasForeignKey(x => x.DriverId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<Vehicle>().WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ShipmentId).HasDatabaseName("IX_trn_tracking_event_ShipmentId");
            entity.HasIndex(x => x.DriverId).HasDatabaseName("IX_trn_tracking_event_DriverId");
        });

        modelBuilder.Entity<Epod>(entity =>
        {
            entity.ToTable("trn_epod");
            entity.HasKey(x => x.EpodId);
            entity.Property(x => x.ReceivedBy).HasMaxLength(150);
            entity.Property(x => x.SignatureFileRef).HasMaxLength(500);
            entity.Property(x => x.PhotoFileRef).HasMaxLength(500);
            entity.Property(x => x.PodStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Remark).HasMaxLength(500);
        });

        modelBuilder.Entity<ShipmentException>(entity =>
        {
            entity.ToTable("trn_exception");
            entity.HasKey(x => x.ExceptionId);
            entity.Property(x => x.ExceptionType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Severity).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ResolutionStatus).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<VehicleMaintenance>(entity =>
        {
            entity.ToTable("trn_vehicle_maintenance");
            entity.HasKey(x => x.VehicleMaintenanceId);
            entity.Property(x => x.MaintenanceType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.OdometerKm).HasPrecision(18, 2);
            entity.Property(x => x.MaintenanceStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Remark).HasMaxLength(500);
        });

        modelBuilder.Entity<FuelTransaction>(entity =>
        {
            entity.ToTable("trn_fuel_transaction");
            entity.HasKey(x => x.FuelTransactionId);
            entity.Property(x => x.FuelLiter).HasPrecision(18, 3);
            entity.Property(x => x.FuelCost).HasPrecision(18, 2);
            entity.Property(x => x.OdometerKm).HasPrecision(18, 2);
            entity.Property(x => x.StationName).HasMaxLength(200);
        });

        modelBuilder.Entity<DriverPerformance>(entity =>
        {
            entity.ToTable("trn_driver_performance");
            entity.HasKey(x => x.DriverPerformanceId);
            entity.Property(x => x.PeriodMonth).IsRequired();
            entity.Property(x => x.OnTimeScore).HasPrecision(5, 2);
            entity.Property(x => x.SafetyScore).HasPrecision(5, 2);
            entity.Property(x => x.FuelEfficiencyScore).HasPrecision(5, 2);
            entity.Property(x => x.PodAccuracyScore).HasPrecision(5, 2);
            entity.Property(x => x.OverallScore).HasPrecision(5, 2);
        });

        modelBuilder.Entity<FreightContract>(entity =>
        {
            entity.ToTable("mst_freight_contract");
            entity.HasKey(x => x.FreightContractId);
            entity.Property(x => x.ContractNo).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ContractName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RateType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.HasIndex(x => x.ContractNo).HasDatabaseName("UX_mst_freight_contract_ContractNo").IsUnique();
        });

        modelBuilder.Entity<FreightAudit>(entity =>
        {
            entity.ToTable("trn_freight_audit");
            entity.HasKey(x => x.FreightAuditId);
            entity.Property(x => x.CarrierInvoiceNo).HasMaxLength(100);
            entity.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            entity.Property(x => x.CarrierAmount).HasPrecision(18, 2);
            entity.Property(x => x.DifferenceAmount).HasPrecision(18, 2);
            entity.Property(x => x.AuditStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.PaymentStatus).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<BillingInvoice>(entity =>
        {
            entity.ToTable("trn_billing_invoice");
            entity.HasKey(x => x.BillingInvoiceId);
            entity.Property(x => x.InvoiceNo).HasMaxLength(100).IsRequired();
            entity.Property(x => x.InvoiceAmount).HasPrecision(18, 2);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.InvoiceStatus).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ErpPostingStatus).HasMaxLength(30);
            entity.HasIndex(x => x.InvoiceNo).HasDatabaseName("UX_trn_billing_invoice_InvoiceNo").IsUnique();
        });

        modelBuilder.Entity<IotDevice>(entity =>
        {
            entity.ToTable("mst_iot_device");
            entity.HasKey(x => x.IotDeviceId);
            entity.Property(x => x.DeviceCode).HasMaxLength(80).IsRequired();
            entity.Property(x => x.DeviceType).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.DeviceCode).HasDatabaseName("UX_mst_iot_device_DeviceCode").IsUnique();
        });

        modelBuilder.Entity<IotSensorReading>(entity =>
        {
            entity.ToTable("trn_iot_sensor_reading");
            entity.HasKey(x => x.IotSensorReadingId);
            entity.Property(x => x.TemperatureC).HasPrecision(8, 2);
            entity.Property(x => x.HumidityPercent).HasPrecision(5, 2);
            entity.Property(x => x.Latitude).HasPrecision(10, 7);
            entity.Property(x => x.Longitude).HasPrecision(10, 7);
        });

        modelBuilder.Entity<EtaPrediction>(entity =>
        {
            entity.ToTable("trn_eta_prediction");
            entity.HasKey(x => x.EtaPredictionId);
            entity.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
            entity.Property(x => x.DelayRiskScore).HasPrecision(5, 2);
            entity.Property(x => x.ModelVersion).HasMaxLength(50);
            entity.Property(x => x.ReasonSummary).HasMaxLength(500);
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(x => typeof(AuditableEntity).IsAssignableFrom(x.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.CreateBy)).HasMaxLength(100).IsRequired();
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.CreateDate)).HasDefaultValueSql("SYSUTCDATETIME()");
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.ReviseBy)).HasMaxLength(100);
            modelBuilder.Entity(entityType.ClrType).Property(nameof(AuditableEntity.IsActive)).HasDefaultValue(true);
        }
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("sec_role");
            entity.HasKey(x => x.RoleId);
            entity.Property(x => x.RoleCode).HasMaxLength(50).IsRequired();
            entity.Property(x => x.RoleName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Permissions);
            entity.HasIndex(x => x.RoleCode).HasDatabaseName("UX_sec_role_RoleCode").IsUnique();
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("sec_user");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.Username).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.HasIndex(x => x.Username).HasDatabaseName("UX_sec_user_Username").IsUnique();
            entity.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Factory).WithMany().HasForeignKey(x => x.FactoryId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
