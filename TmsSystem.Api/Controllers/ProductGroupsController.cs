using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/product-group")]
public sealed class ProductGroupsController(TmsDbContext dbContext) : CrudController<ProductGroup>(dbContext);
