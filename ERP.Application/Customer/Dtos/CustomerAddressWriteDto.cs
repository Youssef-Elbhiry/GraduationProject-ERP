using System.ComponentModel.DataAnnotations;

namespace ERP.Application.Customer.Dtos;

public class CustomerAddressWriteDto
{
    [Required, StringLength(250)]
    public string Line1 { get; set; } = string.Empty;
    [StringLength(250)]
    public string? Line2 { get; set; }
    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
