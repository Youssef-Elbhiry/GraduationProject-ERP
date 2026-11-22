using ERP.Application.Customer.Dtos;

namespace ERP.Application.Customer;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<CustomerDetailsDto?> GetCustomerAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<List<CustomerAddressDto>?> GetCustomerAddressesAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<CustomerAddressDto?> AddCustomerAddressAsync(string customerUserId, CustomerAddressWriteDto dto, CancellationToken cancellationToken = default);
    Task<List<CustomerAddressDto>> GetMyAddressesAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<CustomerAddressDto?> GetMyAddressAsync(string customerUserId, int addressId, CancellationToken cancellationToken = default);
    Task<CustomerAddressDto?> AddMyAddressAsync(string customerUserId, CustomerAddressWriteDto dto, CancellationToken cancellationToken = default);
    Task<CustomerAddressDto?> UpdateMyAddressAsync(string customerUserId, int addressId, CustomerAddressWriteDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteMyAddressAsync(string customerUserId, int addressId, CancellationToken cancellationToken = default);
}
