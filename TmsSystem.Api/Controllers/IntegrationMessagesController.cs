using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/integration-message")]
public sealed class IntegrationMessagesController(TmsDbContext dbContext) : CrudController<IntegrationMessage>(dbContext);
