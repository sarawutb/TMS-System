using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/customer")]
public sealed class CustomersController(TmsDbContext dbContext) : CrudController<Customer>(dbContext);
