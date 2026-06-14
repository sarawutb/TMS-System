using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/driver-performance")]
public sealed class DriverPerformancesController(TmsDbContext dbContext) : CrudController<DriverPerformance>(dbContext);
