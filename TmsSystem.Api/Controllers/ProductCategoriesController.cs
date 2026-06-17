using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/product-category")]
public sealed class ProductCategoriesController(TmsDbContext dbContext) : CrudController<ProductCategory>(dbContext);
