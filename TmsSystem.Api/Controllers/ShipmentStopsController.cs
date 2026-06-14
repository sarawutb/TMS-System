using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/shipment-stop")]
public sealed class ShipmentStopsController(TmsDbContext dbContext) : CrudController<ShipmentStop>(dbContext);
