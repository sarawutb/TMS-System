using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/product")]
public sealed class ProductsController(TmsDbContext dbContext) : CrudController<Product>(dbContext);
