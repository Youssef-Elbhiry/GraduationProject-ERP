namespace ERP.Application.Customer.Dtos;

public class CustomerDetailsDto : CustomerDto
{
    public List<CustomerAddressDto> Addresses { get; set; } = new();
}
