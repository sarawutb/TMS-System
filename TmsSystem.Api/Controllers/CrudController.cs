using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Api.Responses;
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
