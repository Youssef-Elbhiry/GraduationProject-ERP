namespace ERP.Application.SalesOrder.Dtos;

public class SalesOrderLineDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
