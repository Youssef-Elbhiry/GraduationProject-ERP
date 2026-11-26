using ERP.Application.ProductStock.Dtos;
using ERP.Domain.Constants;
using ERP.Domain.Entities;

namespace ERP.Application.ProductStock;

public interface IProductStockService
{
    Task<ProductStockDto> GetAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default);

    Task<List<StockMovementDto>> GetMovementsAsync(
        int? productId,
        int? warehouseId,
        CancellationToken cancellationToken = default);

    Task<OperationResult<ProductStockDto>> AdjustAsync(
        StockAdjustmentDto dto,
        string userId,
        CancellationToken cancellationToken = default);

    Task<List<FulfillmentTaskDto>> GetTasksAsync(
        FulfillmentStatus? status,
        CancellationToken cancellationToken = default);

    Task<FulfillmentTaskDto?> GetTaskAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<OperationResult<FulfillmentTaskDto>> CheckAvailabilityAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task RecheckWaitingTasksAsync(
        int warehouseId,
        CancellationToken cancellationToken = default);

    Task<FulfillmentTaskDto> CheckWithinCurrentTransactionAsync(
        FulfillmentTask task,
        CancellationToken cancellationToken = default);
}
