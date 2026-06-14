using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/freight-audit")]
public sealed class FreightAuditsController(TmsDbContext dbContext) : CrudController<FreightAudit>(dbContext);
