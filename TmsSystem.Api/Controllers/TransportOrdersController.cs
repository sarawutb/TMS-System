using Microsoft.AspNetCore.Mvc;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/transport-order")]
public sealed class TransportOrdersController(TmsDbContext dbContext) : CrudController<TransportOrder>(dbContext)
{
    protected override IQueryable<TransportOrder> ApplyPagedFilters(IQueryable<TransportOrder> query, string? search)
    {
        query = ApplySearchFilter(query, search);
        var priority = Request.Query["priority"].ToString();
        var status = Request.Query["status"].ToString();
        // ponytail: inline conditional filters to reduce verbosity
        if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(o => o.Priority == priority);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);
        return query;
    }
}