using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/district")]
public sealed class DistrictsController(TmsDbContext dbContext) : CrudController<District>(dbContext);
