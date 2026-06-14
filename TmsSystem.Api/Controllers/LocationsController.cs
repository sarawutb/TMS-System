using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/location")]
public sealed class LocationsController(TmsDbContext dbContext) : CrudController<Location>(dbContext);
