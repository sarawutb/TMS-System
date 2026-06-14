using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/iot-device")]
public sealed class IotDevicesController(TmsDbContext dbContext) : CrudController<IotDevice>(dbContext);
