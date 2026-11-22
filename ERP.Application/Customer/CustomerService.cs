using ERP.Application.Customer.Dtos;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;

namespace ERP.Application.Customer;

public class CustomerService(IUnitOfWork unitOfWork) : ICustomerService
{
    public async Task<List<CustomerDto>> GetCustomersAsync(CancellationToken cancellationToken = default) =>
        (await unitOfWork.Customers.GetCustomersAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<CustomerDetailsDto?> GetCustomerAsync(string customerUserId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Customers.GetCustomerByIdAsync(customerUserId, cancellationToken);
        if (user is null) return null;
        var dto = new CustomerDetailsDto { Id = user.Id, Name = user.Name, Email = user.Email, Phone = user.PhoneNumber };
        dto.Addresses = (await unitOfWork.Customers.GetAddressesAsync(customerUserId, cancellationToken)).Select(ToDto).ToList();
        return dto;
    }

    public async Task<List<CustomerAddressDto>?> GetCustomerAddressesAsync(string customerUserId, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Customers.GetCustomerByIdAsync(customerUserId, cancellationToken) is null) return null;
        return (await unitOfWork.Customers.GetAddressesAsync(customerUserId, cancellationToken)).Select(ToDto).ToList();
    }

    public Task<CustomerAddressDto?> AddCustomerAddressAsync(string customerUserId, CustomerAddressWriteDto dto, CancellationToken cancellationToken = default) =>
        AddAddressAsync(customerUserId, dto, cancellationToken);

    public async Task<List<CustomerAddressDto>> GetMyAddressesAsync(string customerUserId, CancellationToken cancellationToken = default) =>
        (await unitOfWork.Customers.GetAddressesAsync(customerUserId, cancellationToken)).Select(ToDto).ToList();

    public async Task<CustomerAddressDto?> GetMyAddressAsync(string customerUserId, int addressId, CancellationToken cancellationToken = default)
    {
        var address = await unitOfWork.Customers.GetAddressAsync(addressId, cancellationToken);
        return address is null || address.CustomerUserId != customerUserId ? null : ToDto(address);
    }

    public Task<CustomerAddressDto?> AddMyAddressAsync(string customerUserId, CustomerAddressWriteDto dto, CancellationToken cancellationToken = default) =>
        AddAddressAsync(customerUserId, dto, cancellationToken);

    private async Task<CustomerAddressDto?> AddAddressAsync(string customerUserId, CustomerAddressWriteDto dto, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Customers.GetCustomerByIdAsync(customerUserId, cancellationToken) is null) return null;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (dto.IsDefault)
        {
            var addresses = await unitOfWork.Customers.GetAddressesAsync(customerUserId, cancellationToken);
            foreach (var address in addresses.Where(x => x.IsDefault)) address.IsDefault = false;
        }
        var entity = new CustomerAddress
        {
            CustomerUserId = customerUserId,
            Line1 = dto.Line1.Trim(),
            Line2 = string.IsNullOrWhiteSpace(dto.Line2) ? null : dto.Line2.Trim(),
            City = dto.City.Trim(),
            IsDefault = dto.IsDefault
        };
        await unitOfWork.Customers.AddAddressAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<CustomerAddressDto?> UpdateMyAddressAsync(string customerUserId, int addressId, CustomerAddressWriteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Customers.GetAddressAsync(addressId, cancellationToken);
        if (entity is null || entity.CustomerUserId != customerUserId) return null;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        if (dto.IsDefault)
        {
            var addresses = await unitOfWork.Customers.GetAddressesAsync(customerUserId, cancellationToken);
            foreach (var address in addresses.Where(x => x.IsDefault && x.Id != addressId)) address.IsDefault = false;
        }
        entity.Line1 = dto.Line1.Trim();
        entity.Line2 = string.IsNullOrWhiteSpace(dto.Line2) ? null : dto.Line2.Trim();
        entity.City = dto.City.Trim();
        entity.IsDefault = dto.IsDefault;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<bool> DeleteMyAddressAsync(string customerUserId, int addressId, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Customers.GetAddressAsync(addressId, cancellationToken);
        if (entity is null || entity.CustomerUserId != customerUserId) return false;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        unitOfWork.Customers.DeleteAddress(entity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static CustomerDto ToDto(AppUser user) => new()
    {
        Id = user.Id, Name = user.Name, Email = user.Email, Phone = user.PhoneNumber
    };

    private static CustomerAddressDto ToDto(CustomerAddress address) => new()
    {
        Id = address.Id, Line1 = address.Line1, Line2 = address.Line2, City = address.City, IsDefault = address.IsDefault
    };
}
