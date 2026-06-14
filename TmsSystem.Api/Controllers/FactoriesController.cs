using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/factory")]
public sealed class FactoriesController(TmsDbContext dbContext) : CrudController<Factory>(dbContext);
