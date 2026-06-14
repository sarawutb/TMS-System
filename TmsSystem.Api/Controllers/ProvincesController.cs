using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/province")]
public sealed class ProvincesController(TmsDbContext dbContext) : CrudController<Province>(dbContext);
