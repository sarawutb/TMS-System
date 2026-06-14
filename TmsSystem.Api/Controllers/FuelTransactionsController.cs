using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/fuel-transaction")]
public sealed class FuelTransactionsController(TmsDbContext dbContext) : CrudController<FuelTransaction>(dbContext);
