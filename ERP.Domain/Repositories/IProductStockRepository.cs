using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface IProductStockRepository
{
    Task<ProductStock?> GetAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default);

    Task<List<ProductStock>> GetByProductsAsync(
        IEnumerable<int> productIds,
        int warehouseId,
        CancellationToken cancellationToken = default);

    Task<List<ProductStockMovement>> GetMovementsAsync(
        int? productId,
        int? warehouseId,
        CancellationToken cancellationToken = default);

    Task AddMovementAsync(
        ProductStockMovement movement,
        CancellationToken cancellationToken = default);
}
