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
        if (!string.IsNullOrWhiteSpace(priority))
        {
            query = query.Where(order => order.Priority == priority);
        }

        var status = Request.Query["status"].ToString();
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(order => order.Status == status);
        }

        return query;
    }
}