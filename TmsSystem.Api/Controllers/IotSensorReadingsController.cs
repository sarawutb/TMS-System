using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/iot-sensor-reading")]
public sealed class IotSensorReadingsController(TmsDbContext dbContext) : CrudController<IotSensorReading>(dbContext);
