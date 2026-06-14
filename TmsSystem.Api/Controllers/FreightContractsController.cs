using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/freight-contract")]
public sealed class FreightContractsController(TmsDbContext dbContext) : CrudController<FreightContract>(dbContext);
