using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/carrier-tender")]
public sealed class CarrierTendersController(TmsDbContext dbContext) : CrudController<CarrierTender>(dbContext);
