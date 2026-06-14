using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/transport-order")]
public sealed class TransportOrdersController(TmsDbContext dbContext) : CrudController<TransportOrder>(dbContext);
