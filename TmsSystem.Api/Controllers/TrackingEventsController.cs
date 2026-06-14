using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/tracking-event")]
public sealed class TrackingEventsController(TmsDbContext dbContext) : CrudController<TrackingEvent>(dbContext);
