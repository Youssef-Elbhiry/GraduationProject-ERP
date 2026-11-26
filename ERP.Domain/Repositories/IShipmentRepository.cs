using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface IShipmentRepository
{
    Task<Shipment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Shipment?> GetByTaskIdAsync(
        int taskId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Shipment shipment,
        CancellationToken cancellationToken = default);
}
