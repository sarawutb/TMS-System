using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsSystem.Domain.Entities;
using TmsSystem.Infrastructure.Data;

namespace TmsSystem.Api.Controllers;

[Route("api/transport-order-item")]
public sealed class TransportOrderItemsController(TmsDbContext dbContext) : CrudController<TransportOrderItem>(dbContext)
{
    public override async Task<IActionResult> Create(TransportOrderItem entity, CancellationToken cancellationToken)
    {
        await ResolveProductUnitAsync(entity, cancellationToken);
        return await base.Create(entity, cancellationToken);
    }

    public override async Task<IActionResult> Update(long id, TransportOrderItem entity, CancellationToken cancellationToken)
    {
        await ResolveProductUnitAsync(entity, cancellationToken);
        return await base.Update(id, entity, cancellationToken);
    }

    private async Task ResolveProductUnitAsync(TransportOrderItem item, CancellationToken cancellationToken)
    {
        if (item.ProductUnitId.HasValue)
        {
            var productUnitId = item.ProductUnitId.Value;
            var productUnit = await Context.ProductUnits.AsNoTracking().FirstOrDefaultAsync(x => x.ProductUnitId == productUnitId, cancellationToken);
            if (productUnit is null) return;
            item.ProductId ??= productUnit.ProductId;
            item.UnitId ??= productUnit.UnitId;
            await FillUnitNameAsync(item, cancellationToken);
            return;
        }

        if (!item.UnitId.HasValue && !string.IsNullOrWhiteSpace(item.UnitName))
        {
            var unitName = item.UnitName.Trim();
            item.UnitId = await Context.Units.AsNoTracking()
                .Where(x => x.UnitCode == unitName || x.UnitSymbol == unitName || x.UnitNameTh == unitName || x.UnitNameEn == unitName || x.UnitNameShort == unitName)
                .Select(x => (long?)x.UnitId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (item.ProductId.HasValue && item.UnitId.HasValue)
        {
            var productId = item.ProductId.Value;
            var unitId = item.UnitId.Value;
            item.ProductUnitId = await Context.ProductUnits.AsNoTracking()
                .Where(x => x.ProductId == productId && x.UnitId == unitId)
                .Select(x => (long?)x.ProductUnitId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        await FillUnitNameAsync(item, cancellationToken);
    }

    private async Task FillUnitNameAsync(TransportOrderItem item, CancellationToken cancellationToken)
    {
        if (!item.UnitId.HasValue || !string.IsNullOrWhiteSpace(item.UnitName)) return;
        var unitId = item.UnitId.Value;
        item.UnitName = await Context.Units.AsNoTracking()
            .Where(x => x.UnitId == unitId)
            .Select(x => x.UnitNameShort ?? x.UnitCode)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
