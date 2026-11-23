using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface IFulfillmentTaskRepository
{
    Task<FulfillmentTask?> GetBySalesOrderIdAsync(int salesOrderId, CancellationToken cancellationToken = default);
    Task AddAsync(FulfillmentTask task, CancellationToken cancellationToken = default);
}
