using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/carrier")]
public sealed class CarriersController(TmsDbContext dbContext) : CrudController<Carrier>(dbContext);
