using ERP.Domain.Constants;

namespace ERP.Domain.Entities;

public class FulfillmentTask
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public int WarehouseId { get; set; }
    public FulfillmentStatus Status { get; set; } = FulfillmentStatus.Waiting;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadyAt { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public Shipment? Shipment { get; set; }
}
