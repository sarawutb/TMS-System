using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/transport-order-item")]
public sealed class TransportOrderItemsController(TmsDbContext dbContext) : CrudController<TransportOrderItem>(dbContext);
