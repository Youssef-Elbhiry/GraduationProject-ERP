namespace ERP.Application.SalesOrder.Dtos;

public class SendSalesOrderResultDto
{
    public SalesOrderDto Order { get; set; } = new();
    public bool EmailSent { get; set; }
}
