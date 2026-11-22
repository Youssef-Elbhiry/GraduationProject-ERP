using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface IWarehouseRepository
{
    Task<List<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetFirstAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    void Update(Warehouse warehouse);
}
