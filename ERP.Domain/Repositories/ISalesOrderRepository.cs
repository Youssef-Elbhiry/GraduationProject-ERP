using ERP.Domain.Constants;
using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface ISalesOrderRepository
{
    Task<SalesOrder?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<SalesOrder>> GetAllAsync(string? customerUserId, SalesOrderStatus? status, CancellationToken cancellationToken = default);
    Task AddAsync(SalesOrder order, CancellationToken cancellationToken = default);
    void ReplaceLines(SalesOrder order, IEnumerable<SalesOrderLine> lines);
}
