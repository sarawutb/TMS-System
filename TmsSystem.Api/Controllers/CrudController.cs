using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Api.Responses;
using TmsSystem.Application.Common;
using TmsSystem.Domain.Common;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[ApiController]
[Authorize]
public abstract class CrudController<TEntity>(TmsDbContext dbContext) : TmsControllerBase where TEntity : class
{
    protected TmsDbContext Context => dbContext;

    [HttpGet]
    public virtual async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => ApiSuccess(await dbContext.Set<TEntity>().AsNoTracking().ToListAsync(cancellationToken));

    [HttpGet("paged")]
    public virtual async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = ApplyPagedFilters(dbContext.Set<TEntity>().AsNoTracking(), search);
        var totalItems = await query.CountAsync(cancellationToken);
        var items = await ApplyDefaultOrdering(query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ApiSuccess(PagedResult<TEntity>.Create(items, totalItems, pageNumber, pageSize));
    }

    [HttpGet("{id:long}")]
    public virtual async Task<IActionResult> GetById(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
        return entity is null
            ? ApiFailure<TEntity>(StatusCodes.Status404NotFound, $"{typeof(TEntity).Name} not found.")
            : ApiSuccess(entity);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Create(TEntity entity, CancellationToken cancellationToken)
    {
        ApplyCreateAudit(entity);
        dbContext.Set<TEntity>().Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = GetPrimaryKeyValue(entity) },
            ApiResponse<TEntity>.SuccessResponse(entity, StatusCodes.Status201Created));
    }

    [HttpPut("{id:long}")]
    public virtual async Task<IActionResult> Update(long id, TEntity entity, CancellationToken cancellationToken)
    {
        if (GetPrimaryKeyValue(entity) != id)
        {
            return ApiFailure<TEntity>(StatusCodes.Status400BadRequest, "Route id does not match entity primary key.");
        }

        ApplyReviseAudit(entity);
        dbContext.Entry(entity).State = EntityState.Modified;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiSuccess(entity);
    }

    [HttpDelete("{id:long}")]
    public virtual async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
        if (entity is null)
        {
            return ApiFailure<TEntity>(StatusCodes.Status404NotFound, $"{typeof(TEntity).Name} not found.");
        }

        dbContext.Set<TEntity>().Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiSuccess(true);
    }

    protected virtual IQueryable<TEntity> ApplyPagedFilters(IQueryable<TEntity> query, string? search)
        => ApplySearchFilter(query, search);

    protected static IQueryable<TEntity> ApplySearchFilter(IQueryable<TEntity> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search)) return query;
        var term = search.Trim().ToLower();
        var param = Expression.Parameter(typeof(TEntity), "e");
        // ponytail: use LINQ Aggregate to build search condition expression cleanly without verbose loops
        var body = typeof(TEntity).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => Expression.Call(
                Expression.Call(Expression.Property(param, p), nameof(string.ToLower), Type.EmptyTypes),
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(term)
            ))
            .Aggregate<Expression, Expression?>(null, (acc, next) => acc == null ? next : Expression.OrElse(acc, next));

        return body == null ? query : query.Where(Expression.Lambda<Func<TEntity, bool>>(body, param));
    }

    private IQueryable<TEntity> ApplyDefaultOrdering(IQueryable<TEntity> query)
    {
        // ponytail: use native EF.Property dynamic ordering, avoiding reflection MakeGenericMethod
        var keyName = dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
        return keyName is null ? query : query.OrderBy(e => EF.Property<long>(e, keyName));
    }

    private long GetPrimaryKeyValue(TEntity entity)
    {
        // ponytail: use EF Entry Metadata and Property API to avoid reflection GetValue
        var keyName = dbContext.Entry(entity).Metadata.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
        return keyName is null ? throw new InvalidOperationException($"Primary key not configured for {typeof(TEntity).Name}.")
            : Convert.ToInt64(dbContext.Entry(entity).Property(keyName).CurrentValue);
    }

    private void ApplyCreateAudit(TEntity entity)
    {
        if (entity is not AuditableEntity a) return;
        a.IsActive = true;
        a.CreateDate = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(a.CreateBy)) a.CreateBy = User.Identity?.Name ?? "api";
    }

    private void ApplyReviseAudit(TEntity entity)
    {
        if (entity is not AuditableEntity a) return;
        a.ReviseDate = DateTime.UtcNow;
        a.ReviseBy = User.Identity?.Name ?? "api";
    }
}