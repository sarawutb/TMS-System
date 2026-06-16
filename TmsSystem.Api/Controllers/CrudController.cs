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
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var normalizedSearch = search.Trim().ToLowerInvariant();
        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        Expression? combined = null;

        foreach (var property in typeof(TEntity).GetProperties().Where(property => property.PropertyType == typeof(string)))
        {
            var propertyAccess = Expression.Property(parameter, property);
            var notNull = Expression.NotEqual(propertyAccess, Expression.Constant(null, typeof(string)));
            var toLower = Expression.Call(propertyAccess, nameof(string.ToLower), Type.EmptyTypes);
            var contains = Expression.Call(toLower, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(normalizedSearch));
            var condition = Expression.AndAlso(notNull, contains);
            combined = combined is null ? condition : Expression.OrElse(combined, condition);
        }

        return combined is null ? query : query.Where(Expression.Lambda<Func<TEntity, bool>>(combined, parameter));
    }

    private IQueryable<TEntity> ApplyDefaultOrdering(IQueryable<TEntity> query)
    {
        var key = dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.SingleOrDefault();
        if (key is null)
        {
            return query;
        }

        var parameter = Expression.Parameter(typeof(TEntity), "entity");
        var property = Expression.Property(parameter, key.Name);
        var lambda = Expression.Lambda(property, parameter);
        var method = typeof(Queryable).GetMethods()
            .Single(method => method.Name == nameof(Queryable.OrderBy)
                && method.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), property.Type);

        return (IQueryable<TEntity>)method.Invoke(null, [query, lambda])!;
    }

    private long GetPrimaryKeyValue(TEntity entity)
    {
        var key = dbContext.Model.FindEntityType(typeof(TEntity))?.FindPrimaryKey()?.Properties.SingleOrDefault() ?? throw new InvalidOperationException($"Primary key not configured for {typeof(TEntity).Name}.");
        return Convert.ToInt64(typeof(TEntity).GetProperty(key.Name)?.GetValue(entity));
    }
    private void ApplyCreateAudit(TEntity entity)
    {
        if (entity is not AuditableEntity auditable) return; auditable.IsActive = true; auditable.CreateDate = DateTime.UtcNow; if (string.IsNullOrWhiteSpace(auditable.CreateBy)) auditable.CreateBy = User.Identity?.Name ?? "api";
    }
    private void ApplyReviseAudit(TEntity entity)
    {
        if (entity is not AuditableEntity auditable) return; auditable.ReviseDate = DateTime.UtcNow; auditable.ReviseBy = User.Identity?.Name ?? "api";
    }
}