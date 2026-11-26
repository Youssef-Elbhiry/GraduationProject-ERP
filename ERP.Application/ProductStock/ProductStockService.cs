using ERP.Application.Interfaces;
using ERP.Application.ProductStock.Dtos;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ProductStockEntity = ERP.Domain.Entities.ProductStock;

namespace ERP.Application.ProductStock;

public class ProductStockService(IUnitOfWork unitOfWork) : IProductStockService
{
    public async Task<ProductStockDto> GetAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        var stock = await unitOfWork.ProductStocks.GetAsync(
            productId,
            warehouseId,
            cancellationToken);
        var reservedQty = await GetReservedQtyAsync(
            productId,
            warehouseId,
            cancellationToken);
        var onHandQty = stock?.OnHandQty ?? 0m;

        return new ProductStockDto(
            productId,
            warehouseId,
            onHandQty,
            reservedQty,
            onHandQty - reservedQty);
    }

    public async Task<List<StockMovementDto>> GetMovementsAsync(
        int? productId,
        int? warehouseId,
        CancellationToken cancellationToken = default)
    {
        var movements = await unitOfWork.ProductStocks.GetMovementsAsync(
            productId,
            warehouseId,
            cancellationToken);

        return movements
            .Select(movement => new StockMovementDto(
                movement.Id,
                movement.ProductId,
                movement.WarehouseId,
                movement.Type,
                movement.QtyChange,
                movement.MovedAt,
                movement.UserId,
                movement.Note,
                movement.ShipmentId))
            .ToList();
    }

    public async Task<OperationResult<ProductStockDto>> AdjustAsync(
        StockAdjustmentDto dto,
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (dto.ProductId <= 0
            || dto.WarehouseId <= 0
            || dto.QtyChange == 0
            || dto.Type is not (ProductMovementType.StockIn or ProductMovementType.Adjustment)
            || (dto.Type == ProductMovementType.StockIn && dto.QtyChange < 0))
        {
            return new OperationResult<ProductStockDto>(
                null,
                400,
                "Product, warehouse, non-zero quantity, and StockIn or Adjustment type are required. "
                + "StockIn quantity must be positive.");
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var stock = await unitOfWork.ProductStocks.GetAsync(
            dto.ProductId,
            dto.WarehouseId,
            cancellationToken);

        if (stock is null)
        {
            stock = new ProductStockEntity
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                RowVersion = Guid.NewGuid().ToByteArray()
            };

            await unitOfWork.Repository<ProductStockEntity>().AddAsync(stock);
        }

        if (stock.OnHandQty + dto.QtyChange < 0)
        {
            return new OperationResult<ProductStockDto>(
                null,
                400,
                "On-hand stock cannot be negative.");
        }

        stock.OnHandQty += dto.QtyChange;
        stock.RowVersion = Guid.NewGuid().ToByteArray();

        var movement = new ProductStockMovement
        {
            ProductId = dto.ProductId,
            WarehouseId = dto.WarehouseId,
            Type = dto.Type,
            QtyChange = dto.QtyChange,
            MovedAt = DateTime.UtcNow,
            UserId = userId,
            Note = dto.Note
        };

        await unitOfWork.ProductStocks.AddMovementAsync(movement, cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new OperationResult<ProductStockDto>(
                null,
                409,
                "Stock was changed by someone else, please retry");
        }

        if (dto.QtyChange > 0)
        {
            await RecheckWaitingTasksCoreAsync(dto.WarehouseId, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new OperationResult<ProductStockDto>(
            await GetAsync(dto.ProductId, dto.WarehouseId, cancellationToken));
    }

    public async Task<List<FulfillmentTaskDto>> GetTasksAsync(
        FulfillmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var tasks = await unitOfWork.FulfillmentTasks.GetWarehouseTasksAsync(
            null,
            status,
            cancellationToken);
        var result = new List<FulfillmentTaskDto>();

        foreach (var task in tasks)
        {
            result.Add(await BuildTaskDtoAsync(task, cancellationToken));
        }

        return result;
    }

    public async Task<FulfillmentTaskDto?> GetTaskAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var task = await unitOfWork.FulfillmentTasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        return await BuildTaskDtoAsync(task, cancellationToken);
    }

    public async Task<OperationResult<FulfillmentTaskDto>> CheckAvailabilityAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginSerializableTransactionAsync(
            cancellationToken);
        var task = await unitOfWork.FulfillmentTasks.GetByIdAsync(id, cancellationToken);

        if (task is null)
        {
            return new OperationResult<FulfillmentTaskDto>(
                null,
                404,
                "Fulfillment task not found.");
        }

        if (task.Status is FulfillmentStatus.Cancelled or FulfillmentStatus.Done)
        {
            return new OperationResult<FulfillmentTaskDto>(
                null,
                409,
                "Only Waiting or Ready tasks can be checked.");
        }

        if (task.Status == FulfillmentStatus.Ready)
        {
            return new OperationResult<FulfillmentTaskDto>(
                await BuildTaskDtoAsync(task, cancellationToken));
        }

        var availableByProduct = await GetAvailableByProductAsync(task, cancellationToken);
        var lines = BuildTaskLines(task, availableByProduct);

        if (lines.Any(line => line.MissingQty > 0))
        {
            await transaction.CommitAsync(cancellationToken);

            var waitingTask = new FulfillmentTaskDto(
                task.Id,
                task.SalesOrderId,
                task.WarehouseId,
                task.Status.ToString(),
                task.CreatedAt,
                task.ReadyAt,
                lines);

            return new OperationResult<FulfillmentTaskDto>(waitingTask);
        }

        task.Status = FulfillmentStatus.Ready;
        task.ReadyAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new OperationResult<FulfillmentTaskDto>(
            BuildTaskDto(task, availableByProduct));
    }

    public async Task RecheckWaitingTasksAsync(
        int warehouseId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginSerializableTransactionAsync(
            cancellationToken);

        await RecheckWaitingTasksCoreAsync(warehouseId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<FulfillmentTaskDto> CheckWithinCurrentTransactionAsync(
        FulfillmentTask task,
        CancellationToken cancellationToken = default)
    {
        var availableByProduct = await GetAvailableByProductAsync(task, cancellationToken);
        var lines = BuildTaskLines(task, availableByProduct);

        if (lines.All(line => line.MissingQty == 0))
        {
            task.Status = FulfillmentStatus.Ready;
            task.ReadyAt = DateTime.UtcNow;
        }

        return BuildTaskDto(task, availableByProduct);
    }

    private async Task RecheckWaitingTasksCoreAsync(
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var waitingTasks = await unitOfWork.FulfillmentTasks.GetWaitingOldestFirstAsync(
            warehouseId,
            cancellationToken);

        foreach (var task in waitingTasks)
        {
            var availableByProduct = await GetAvailableByProductAsync(task, cancellationToken);
            var lines = BuildTaskLines(task, availableByProduct);

            if (lines.Any(line => line.MissingQty > 0))
            {
                continue;
            }

            task.Status = FulfillmentStatus.Ready;
            task.ReadyAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<FulfillmentTaskDto> BuildTaskDtoAsync(
        FulfillmentTask task,
        CancellationToken cancellationToken)
    {
        var availableByProduct = await GetAvailableByProductAsync(task, cancellationToken);
        return BuildTaskDto(task, availableByProduct);
    }

    private async Task<Dictionary<int, decimal>> GetAvailableByProductAsync(
        FulfillmentTask task,
        CancellationToken cancellationToken)
    {
        var productIds = task.SalesOrder.Lines
            .Select(line => line.ProductId)
            .Distinct()
            .ToList();
        var stocks = await unitOfWork.ProductStocks.GetByProductsAsync(
            productIds,
            task.WarehouseId,
            cancellationToken);
        var readyTasks = await unitOfWork.FulfillmentTasks.GetWarehouseTasksAsync(
            task.WarehouseId,
            FulfillmentStatus.Ready,
            cancellationToken);

        return productIds.ToDictionary(
            productId => productId,
            productId =>
            {
                var onHandQty = stocks
                    .FirstOrDefault(stock => stock.ProductId == productId)
                    ?.OnHandQty ?? 0m;
                var reservedQty = readyTasks
                    .SelectMany(readyTask => readyTask.SalesOrder.Lines)
                    .Where(line => line.ProductId == productId)
                    .Sum(line => line.Qty);

                return onHandQty - reservedQty;
            });
    }

    private static List<FulfillmentLineAvailabilityDto> BuildTaskLines(
        FulfillmentTask task,
        Dictionary<int, decimal> availableByProduct)
    {
        return task.SalesOrder.Lines
            .GroupBy(line => line.ProductId)
            .Select(group =>
            {
                var requiredQty = group.Sum(line => line.Qty);
                var availableQty = availableByProduct.GetValueOrDefault(group.Key);
                var missingQty = Math.Max(0m, requiredQty - availableQty);

                return new FulfillmentLineAvailabilityDto(
                    group.Key,
                    requiredQty,
                    availableQty,
                    missingQty);
            })
            .ToList();
    }

    private static FulfillmentTaskDto BuildTaskDto(
        FulfillmentTask task,
        Dictionary<int, decimal> availableByProduct)
    {
        return new FulfillmentTaskDto(
            task.Id,
            task.SalesOrderId,
            task.WarehouseId,
            task.Status.ToString(),
            task.CreatedAt,
            task.ReadyAt,
            BuildTaskLines(task, availableByProduct));
    }

    private async Task<decimal> GetReservedQtyAsync(
        int productId,
        int warehouseId,
        CancellationToken cancellationToken)
    {
        var readyTasks = await unitOfWork.FulfillmentTasks.GetWarehouseTasksAsync(
            warehouseId,
            FulfillmentStatus.Ready,
            cancellationToken);

        return readyTasks
            .SelectMany(task => task.SalesOrder.Lines)
            .Where(line => line.ProductId == productId)
            .Sum(line => line.Qty);
    }
}
