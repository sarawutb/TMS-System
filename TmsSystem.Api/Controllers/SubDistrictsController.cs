using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/subdistrict")]
public sealed class SubDistrictsController(TmsDbContext dbContext) : CrudController<SubDistrict>(dbContext);
