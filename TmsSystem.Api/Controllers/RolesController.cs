using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/roles")]
public sealed class RolesController(TmsDbContext dbContext) : CrudController<Role>(dbContext);
