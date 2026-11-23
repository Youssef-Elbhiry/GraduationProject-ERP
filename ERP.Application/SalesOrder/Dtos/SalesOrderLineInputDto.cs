using System.ComponentModel.DataAnnotations;

namespace ERP.Application.SalesOrder.Dtos;

public class SalesOrderLineInputDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.001", "999999999999999.999")]
    public decimal Qty { get; set; }
}
