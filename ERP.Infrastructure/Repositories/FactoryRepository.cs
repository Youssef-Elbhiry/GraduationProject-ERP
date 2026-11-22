using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class FactoryRepository(ErpDbContext context) : IFactoryRepository
{
    public Task<List<Factory>> GetAllAsync(CancellationToken cancellationToken = default) =>
        context.Factories.Include(x => x.Warehouse).OrderBy(x => x.Id).ToListAsync(cancellationToken);
    public Task<Factory?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        context.Factories.Include(x => x.Warehouse).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<Factory?> GetFirstAsync(CancellationToken cancellationToken = default) =>
        context.Factories.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
    public Task AddAsync(Factory factory, CancellationToken cancellationToken = default) =>
        context.Factories.AddAsync(factory, cancellationToken).AsTask();
    public void Update(Factory factory) => context.Factories.Update(factory);
    public Task<bool> WarehouseExistsAsync(int warehouseId, CancellationToken cancellationToken = default) =>
        context.Warehouses.AnyAsync(x => x.Id == warehouseId, cancellationToken);
}
