using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class ShipmentRepository(ErpDbContext context) : IShipmentRepository
{
    public Task<Shipment?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return context.Shipments
            .Include(shipment => shipment.FulfillmentTask)
                .ThenInclude(task => task.SalesOrder)
                    .ThenInclude(order => order.Lines)
            .FirstOrDefaultAsync(shipment => shipment.Id == id, cancellationToken);
    }

    public Task<Shipment?> GetByTaskIdAsync(
        int taskId,
        CancellationToken cancellationToken = default)
    {
        return context.Shipments
            .FirstOrDefaultAsync(
                shipment => shipment.FulfillmentTaskId == taskId,
                cancellationToken);
    }

    public async Task AddAsync(
        Shipment shipment,
        CancellationToken cancellationToken = default)
    {
        await context.Shipments.AddAsync(shipment, cancellationToken);
    }
}
