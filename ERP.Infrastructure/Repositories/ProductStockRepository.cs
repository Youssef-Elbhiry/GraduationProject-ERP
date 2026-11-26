using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class ProductStockRepository(ErpDbContext context) : IProductStockRepository
{
    public Task<ProductStock?> GetAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        return context.ProductStocks.FirstOrDefaultAsync(
            stock => stock.ProductId == productId && stock.WarehouseId == warehouseId,
            cancellationToken);
    }

    public Task<List<ProductStock>> GetByProductsAsync(
        IEnumerable<int> productIds,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        return context.ProductStocks
            .Where(stock => stock.WarehouseId == warehouseId
                && productIds.Contains(stock.ProductId))
            .ToListAsync(cancellationToken);
    }

    public Task<List<ProductStockMovement>> GetMovementsAsync(
        int? productId,
        int? warehouseId,
        CancellationToken cancellationToken = default)
    {
        var query = context.ProductStockMovements
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(movement => movement.ProductId == productId.Value);
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(movement => movement.WarehouseId == warehouseId.Value);
        }

        return query
            .OrderByDescending(movement => movement.MovedAt)
            .ThenByDescending(movement => movement.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddMovementAsync(
        ProductStockMovement movement,
        CancellationToken cancellationToken = default)
    {
        await context.ProductStockMovements.AddAsync(movement, cancellationToken);
    }
}
