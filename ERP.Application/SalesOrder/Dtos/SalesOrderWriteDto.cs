using System.ComponentModel.DataAnnotations;

namespace ERP.Application.SalesOrder.Dtos;

public class SalesOrderWriteDto
{
    [Required]
    public string CustomerUserId { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ShippingAddressId { get; set; }

    public DateTime ValidUntil { get; set; }

    [Required]
    public List<SalesOrderLineInputDto> Lines { get; set; } = new();
}
