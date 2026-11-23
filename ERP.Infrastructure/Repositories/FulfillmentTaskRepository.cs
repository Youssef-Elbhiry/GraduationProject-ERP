using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class FulfillmentTaskRepository(ErpDbContext context) : IFulfillmentTaskRepository
{
    public Task<FulfillmentTask?> GetBySalesOrderIdAsync(int salesOrderId, CancellationToken cancellationToken = default) =>
        context.FulfillmentTasks.FirstOrDefaultAsync(x => x.SalesOrderId == salesOrderId, cancellationToken);

    public async Task AddAsync(FulfillmentTask task, CancellationToken cancellationToken = default) =>
        await context.FulfillmentTasks.AddAsync(task, cancellationToken);
}
