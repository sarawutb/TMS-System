using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/exception")]
public sealed class ExceptionsController(TmsDbContext dbContext) : CrudController<ShipmentException>(dbContext);
