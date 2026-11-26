using ERP.Domain.Constants;
using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface IFulfillmentTaskRepository
{
    Task<FulfillmentTask?> GetBySalesOrderIdAsync(
        int salesOrderId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FulfillmentTask task,
        CancellationToken cancellationToken = default);

    Task<List<FulfillmentTask>> GetWarehouseTasksAsync(
        int? warehouseId,
        FulfillmentStatus? status,
        CancellationToken cancellationToken = default);

    Task<FulfillmentTask?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<List<FulfillmentTask>> GetWaitingOldestFirstAsync(
        int warehouseId,
        CancellationToken cancellationToken = default);
}
