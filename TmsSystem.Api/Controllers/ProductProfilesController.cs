using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/product-profile")]
public sealed class ProductProfilesController(TmsDbContext dbContext) : CrudController<ProductProfile>(dbContext);
