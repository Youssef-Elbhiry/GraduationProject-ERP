using ERP.Domain.Constants;

namespace ERP.Domain.Entities;

public class SalesOrder
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string CustomerUserId { get; set; } = string.Empty;
    public int ShippingAddressId { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public AppUser CustomerUser { get; set; } = null!;
    public CustomerAddress ShippingAddress { get; set; } = null!;
    public AppUser CreatedByUser { get; set; } = null!;
    public ICollection<SalesOrderLine> Lines { get; set; } = new List<SalesOrderLine>();
    public FulfillmentTask? FulfillmentTask { get; set; }
}
