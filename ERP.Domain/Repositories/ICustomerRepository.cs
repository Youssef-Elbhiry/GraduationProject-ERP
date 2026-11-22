using ERP.Domain.Entities;

namespace ERP.Domain.Repositories;

public interface ICustomerRepository
{
    Task<List<AppUser>> GetCustomersAsync(CancellationToken cancellationToken = default);
    Task<AppUser?> GetCustomerByIdAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<List<CustomerAddress>> GetAddressesAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<CustomerAddress?> GetAddressAsync(int addressId, CancellationToken cancellationToken = default);
    Task AddAddressAsync(CustomerAddress address, CancellationToken cancellationToken = default);
    void UpdateAddress(CustomerAddress address);
    void DeleteAddress(CustomerAddress address);
}
