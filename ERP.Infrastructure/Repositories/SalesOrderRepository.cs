using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class SalesOrderRepository(ErpDbContext context) : ISalesOrderRepository
{
    public Task<SalesOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.SalesOrders.Include(x => x.Lines).Include(x => x.CustomerUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<SalesOrder>> GetAllAsync(string? customerUserId, SalesOrderStatus? status, CancellationToken cancellationToken = default)
    {
        IQueryable<SalesOrder> query = context.SalesOrders.AsNoTracking().Include(x => x.Lines);
        if (!string.IsNullOrWhiteSpace(customerUserId)) query = query.Where(x => x.CustomerUserId == customerUserId);
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        return query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync(cancellationToken);
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
