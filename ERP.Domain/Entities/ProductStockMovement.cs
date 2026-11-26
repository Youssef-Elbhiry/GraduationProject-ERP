using ERP.Domain.Constants;

namespace ERP.Domain.Entities;

public class ProductStockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int WarehouseId { get; set; }
    public ProductMovementType Type { get; set; }
    public decimal QtyChange { get; set; }
    public DateTime MovedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? ShipmentId { get; set; }
    public Product Product { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public AppUser User { get; set; } = null!;
    public Shipment? Shipment { get; set; }
}
