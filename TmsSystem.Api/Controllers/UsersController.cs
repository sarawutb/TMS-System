using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/users")]
public sealed class UsersController(TmsDbContext dbContext) : CrudController<User>(dbContext);
