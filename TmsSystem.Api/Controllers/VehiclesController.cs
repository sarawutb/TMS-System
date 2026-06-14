using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/vehicle")]
public sealed class VehiclesController(TmsDbContext dbContext) : CrudController<Vehicle>(dbContext);
