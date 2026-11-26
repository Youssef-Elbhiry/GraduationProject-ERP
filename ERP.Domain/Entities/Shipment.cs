namespace ERP.Domain.Entities;

public class Shipment
{
    public int Id { get; set; }
    public string ShipmentNo { get; set; } = string.Empty;
    public int FulfillmentTaskId { get; set; }
    public string Carrier { get; set; } = string.Empty;
    public string? TrackingNo { get; set; }
    public DateTime ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string ShippedByUserId { get; set; } = string.Empty;
    public FulfillmentTask FulfillmentTask { get; set; } = null!;
    public AppUser ShippedByUser { get; set; } = null!;
    public ICollection<ProductStockMovement> Movements { get; set; } = new List<ProductStockMovement>();
}
