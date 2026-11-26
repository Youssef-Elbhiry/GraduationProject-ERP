using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class FulfillmentTaskRepository(ErpDbContext context) : IFulfillmentTaskRepository
{
    public Task<FulfillmentTask?> GetBySalesOrderIdAsync(
        int salesOrderId,
        CancellationToken cancellationToken = default)
    {
        return context.FulfillmentTasks
            .Include(task => task.SalesOrder)
                .ThenInclude(order => order.Lines)
            .FirstOrDefaultAsync(
                task => task.SalesOrderId == salesOrderId,
                cancellationToken);
    }

    public async Task AddAsync(
        FulfillmentTask task,
        CancellationToken cancellationToken = default)
    {
        await context.FulfillmentTasks.AddAsync(task, cancellationToken);
    }

    public Task<FulfillmentTask?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return context.FulfillmentTasks
            .Include(task => task.SalesOrder)
                .ThenInclude(order => order.Lines)
            .FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
    }

    public Task<List<FulfillmentTask>> GetWarehouseTasksAsync(
        int? warehouseId,
        FulfillmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = context.FulfillmentTasks
            .AsNoTracking()
            .Include(task => task.SalesOrder)
                .ThenInclude(order => order.Lines)
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(task => task.WarehouseId == warehouseId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(task => task.Status == status.Value);
        }

        return query
            .OrderBy(task => task.CreatedAt)
            .ThenBy(task => task.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<List<FulfillmentTask>> GetWaitingOldestFirstAsync(
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        return context.FulfillmentTasks
            .Include(task => task.SalesOrder)
                .ThenInclude(order => order.Lines)
            .Where(task => task.WarehouseId == warehouseId
                && task.Status == FulfillmentStatus.Waiting)
            .OrderBy(task => task.CreatedAt)
            .ThenBy(task => task.Id)
            .ToListAsync(cancellationToken);
    }
}
