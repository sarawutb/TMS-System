using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/route-plan")]
public sealed class RoutePlansController(TmsDbContext dbContext) : CrudController<RoutePlan>(dbContext);
