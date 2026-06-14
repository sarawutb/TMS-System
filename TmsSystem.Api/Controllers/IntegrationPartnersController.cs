using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/integration-partner")]
public sealed class IntegrationPartnersController(TmsDbContext dbContext) : CrudController<IntegrationPartner>(dbContext);
