namespace ERP.Application.SalesOrder.Dtos;

public class SalesOrderDto
{
    public int Id { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public string CustomerUserId { get; set; } = string.Empty;
    public int ShippingAddressId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ValidUntil { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public List<SalesOrderLineDto> Lines { get; set; } = new();
    public decimal Total { get; set; }
    public bool IsExpired { get; set; }
    public string? DeliveryState { get; set; }
    public string? InvoiceState { get; set; }
    public string? PaymentState { get; set; }
}
