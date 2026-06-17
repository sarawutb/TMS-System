using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/unit")]
public sealed class UnitsController(TmsDbContext dbContext) : CrudController<Unit>(dbContext);
