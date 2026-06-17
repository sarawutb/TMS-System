using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/profile")]
public sealed class ProfilesController(TmsDbContext dbContext) : CrudController<Profile>(dbContext)
{
    protected override IQueryable<Profile> ApplyPagedFilters(IQueryable<Profile> query, string? search)
    {
        query = base.ApplyPagedFilters(query, search);
        var scope = Request.Query["scope"].ToString();
        return string.IsNullOrWhiteSpace(scope) ? query : query.Where(x => x.ProfileScope == scope);
    }
}
