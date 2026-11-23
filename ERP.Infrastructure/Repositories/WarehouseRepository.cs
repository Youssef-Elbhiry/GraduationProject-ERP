using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class WarehouseRepository(ErpDbContext context) : IWarehouseRepository
{
    public Task<List<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default) =>
        context.Warehouses.OrderBy(x => x.Id).ToListAsync(cancellationToken);
    public Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Warehouses.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<Warehouse?> GetFirstAsync(CancellationToken cancellationToken = default) =>
        context.Warehouses.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
    public Task<Warehouse?> GetFirstActiveAsync(CancellationToken cancellationToken = default) =>
        context.Warehouses.Where(x => x.IsActive).OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
    public Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default) =>
        context.Warehouses.AddAsync(warehouse, cancellationToken).AsTask();
    public void Update(Warehouse warehouse) => context.Warehouses.Update(warehouse);
}
