using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/load-plan")]
public sealed class LoadPlansController(TmsDbContext dbContext) : CrudController<LoadPlan>(dbContext);
