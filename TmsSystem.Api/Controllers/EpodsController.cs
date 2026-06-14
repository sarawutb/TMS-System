using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/epod")]
public sealed class EpodsController(TmsDbContext dbContext) : CrudController<Epod>(dbContext);
