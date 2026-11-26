using ERP.Application.Interfaces;
using ERP.Application.ProductStock.Dtos;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP.Application.ProductStock;

public class ShipmentService(IUnitOfWork unitOfWork)
{
    public async Task<OperationResult<ShipmentDto>> CreateAsync(
        ShipmentCreateDto dto,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Carrier)
            || dto.Carrier.Length > 100
            || dto.TrackingNo?.Length > 100)
        {
            return new OperationResult<ShipmentDto>(
                null,
                400,
                "Carrier is required and tracking number must be at most 100 characters.");
        }

        await using var transaction = await unitOfWork.BeginSerializableTransactionAsync(
            cancellationToken);
        var task = await unitOfWork.FulfillmentTasks.GetByIdAsync(
            dto.FulfillmentTaskId,
            cancellationToken);

        if (task is null)
        {
            return new OperationResult<ShipmentDto>(
                null,
                404,
                "Fulfillment task not found.");
        }

        if (task.Status != FulfillmentStatus.Ready)
        {
            return new OperationResult<ShipmentDto>(
                null,
                409,
                "The fulfillment task must be Ready before it can ship.");
        }

        var existingShipment = await unitOfWork.Shipments.GetByTaskIdAsync(
            task.Id,
            cancellationToken);
        if (existingShipment is not null)
        {
            return new OperationResult<ShipmentDto>(
                null,
                409,
                "This fulfillment task already has a shipment.");
        }

        var shippedAt = DateTime.UtcNow;
        var shipment = new Shipment
        {
            ShipmentNo = $"SHP-{shippedAt:yyyyMMdd}-{Guid.NewGuid():N}"[..24],
            FulfillmentTaskId = task.Id,
            Carrier = dto.Carrier.Trim(),
            TrackingNo = dto.TrackingNo,
            ShippedAt = shippedAt,
            ShippedByUserId = userId
        };

        await unitOfWork.Shipments.AddAsync(shipment, cancellationToken);

        foreach (var productLines in task.SalesOrder.Lines.GroupBy(line => line.ProductId))
        {
            var stock = await unitOfWork.ProductStocks.GetAsync(
                productLines.Key,
                task.WarehouseId,
                cancellationToken);
            var quantity = productLines.Sum(line => line.Qty);

            if (stock is null || stock.OnHandQty < quantity)
            {
                return new OperationResult<ShipmentDto>(
                    null,
                    409,
                    "Insufficient on-hand stock to ship this order.");
            }

            stock.OnHandQty -= quantity;
            stock.RowVersion = Guid.NewGuid().ToByteArray();

            var movement = new ProductStockMovement
            {
                ProductId = productLines.Key,
                WarehouseId = task.WarehouseId,
                Type = ProductMovementType.Shipment,
                QtyChange = -quantity,
                MovedAt = DateTime.UtcNow,
                UserId = userId,
                Shipment = shipment
            };

            await unitOfWork.ProductStocks.AddMovementAsync(movement, cancellationToken);
        }

        task.Status = FulfillmentStatus.Done;

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new OperationResult<ShipmentDto>(
                null,
                409,
                "Stock was changed by someone else, please retry");
        }

        var result = ToDto(shipment);
        return new OperationResult<ShipmentDto>(result);
    }

    public async Task<OperationResult<ShipmentDto>> DeliveredAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var shipment = await unitOfWork.Shipments.GetByIdAsync(id, cancellationToken);
        if (shipment is null)
        {
            return new OperationResult<ShipmentDto>(
                null,
                404,
                "Shipment not found.");
        }

        shipment.DeliveredAt ??= DateTime.UtcNow;
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new OperationResult<ShipmentDto>(ToDto(shipment));
    }

    public async Task<ShipmentDto?> GetAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var shipment = await unitOfWork.Shipments.GetByIdAsync(id, cancellationToken);
        return shipment is null ? null : ToDto(shipment);
    }

    private static ShipmentDto ToDto(Shipment shipment)
    {
        return new ShipmentDto(
            shipment.Id,
            shipment.ShipmentNo,
            shipment.FulfillmentTaskId,
            shipment.Carrier,
            shipment.TrackingNo,
            shipment.ShippedAt,
            shipment.DeliveredAt,
            shipment.ShippedByUserId);
    }
}
