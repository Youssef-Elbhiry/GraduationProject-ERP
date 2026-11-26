using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class SalesOrderRepository(ErpDbContext context) : ISalesOrderRepository
{
    public Task<SalesOrder?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return context.SalesOrders
            .Include(order => order.Lines)
            .Include(order => order.CustomerUser)
            .Include(order => order.FulfillmentTask)
            .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public Task<List<SalesOrder>> GetAllAsync(
        string? customerUserId,
        SalesOrderStatus? status,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SalesOrder> query = context.SalesOrders
            .AsNoTracking()
            .Include(order => order.Lines)
            .Include(order => order.FulfillmentTask);

        if (!string.IsNullOrWhiteSpace(customerUserId))
        {
            query = query.Where(order => order.CustomerUserId == customerUserId);
        }

        if (status.HasValue)
        {
            query = query.Where(order => order.Status == status.Value);
        }

        return query
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(SalesOrder order, CancellationToken cancellationToken = default) =>
        await context.SalesOrders.AddAsync(order, cancellationToken);

    public void ReplaceLines(SalesOrder order, IEnumerable<SalesOrderLine> lines)
    {
        context.SalesOrderLines.RemoveRange(order.Lines);
        var replacements = lines.ToList();
        context.SalesOrderLines.AddRange(replacements);
        order.Lines = replacements;
    }
}
