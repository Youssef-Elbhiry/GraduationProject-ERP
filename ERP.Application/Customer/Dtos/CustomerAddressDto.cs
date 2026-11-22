namespace ERP.Application.Customer.Dtos;

public class CustomerAddressDto
{
    public int Id { get; set; }
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
