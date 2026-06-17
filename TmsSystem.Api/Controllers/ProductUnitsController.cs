using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/product-unit")]
public sealed class ProductUnitsController(TmsDbContext dbContext) : CrudController<ProductUnit>(dbContext);
