using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface IFactoryRepository
{
    Task<List<Factory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Factory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Factory?> GetFirstAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Factory factory, CancellationToken cancellationToken = default);
    void Update(Factory factory);
    Task<bool> WarehouseExistsAsync(int warehouseId, CancellationToken cancellationToken = default);
}
