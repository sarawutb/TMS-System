using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/vehicle-maintenance")]
public sealed class VehicleMaintenancesController(TmsDbContext dbContext) : CrudController<VehicleMaintenance>(dbContext);
