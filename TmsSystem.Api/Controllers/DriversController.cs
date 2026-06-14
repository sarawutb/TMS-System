using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/driver")]
public sealed class DriversController(TmsDbContext dbContext) : CrudController<Driver>(dbContext);
