using ERP.Domain.Constants;

namespace ERP.Application.ProductStock.Dtos;

public record ProductStockDto(
    int ProductId,
    int WarehouseId,
    decimal OnHandQty,
    decimal ReservedQty,
    decimal AvailableQty);

public record StockAdjustmentDto(
    int ProductId,
    int WarehouseId,
    decimal QtyChange,
    ProductMovementType Type,
    string? Note);

public record StockMovementDto(
    int Id,
    int ProductId,
    int WarehouseId,
    ProductMovementType Type,
    decimal QtyChange,
    DateTime MovedAt,
    string UserId,
    string? Note,
    int? ShipmentId);

public record FulfillmentLineAvailabilityDto(
    int ProductId,
    decimal RequiredQty,
    decimal AvailableQty,
    decimal MissingQty);

public record FulfillmentTaskDto(
    int Id,
    int SalesOrderId,
    int WarehouseId,
    string Status,
    DateTime CreatedAt,
    DateTime? ReadyAt,
    List<FulfillmentLineAvailabilityDto> Lines);

public record ShipmentCreateDto(
    int FulfillmentTaskId,
    string Carrier,
    string? TrackingNo);

public record ShipmentDto(
    int Id,
    string ShipmentNo,
    int FulfillmentTaskId,
    string Carrier,
    string? TrackingNo,
    DateTime ShippedAt,
    DateTime? DeliveredAt,
    string ShippedByUserId);

public record OperationResult<T>(
    T? Value,
    int StatusCode = 200,
    string? Error = null)
{
    public bool Succeeded => Value is not null;
}
