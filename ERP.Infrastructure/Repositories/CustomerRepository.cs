using ERP.Domain.Entities;
using ERP.Domain.Constants;
using ERP.Domain.Repositories;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class CustomerRepository(ErpDbContext context) : ICustomerRepository
{
    public Task<List<AppUser>> GetCustomersAsync(CancellationToken cancellationToken = default) =>
        (from user in context.Users
         join userRole in context.UserRoles on user.Id equals userRole.UserId
         join role in context.Roles on userRole.RoleId equals role.Id
         where role.Name == UserRole.Customer
         select user).Distinct().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task<AppUser?> GetCustomerByIdAsync(string customerUserId, CancellationToken cancellationToken = default) =>
        (from user in context.Users
         join userRole in context.UserRoles on user.Id equals userRole.UserId
         join role in context.Roles on userRole.RoleId equals role.Id
         where role.Name == UserRole.Customer && user.Id == customerUserId
         select user).FirstOrDefaultAsync(cancellationToken);

    public async Task<AppUser?> GetActiveCustomerByIdAsync(string customerUserId, CancellationToken cancellationToken = default)
    {
        var user = await (from candidate in context.Users
         join userRole in context.UserRoles on candidate.Id equals userRole.UserId
         join role in context.Roles on userRole.RoleId equals role.Id
         where role.Name == UserRole.Customer && candidate.Id == customerUserId
         select candidate).FirstOrDefaultAsync(cancellationToken);

        return user is not null && (!user.LockoutEnabled || user.LockoutEnd is null || user.LockoutEnd <= DateTimeOffset.UtcNow)
            ? user
            : null;
    }

    public Task<List<CustomerAddress>> GetAddressesAsync(string customerUserId, CancellationToken cancellationToken = default) =>
        context.CustomerAddresses.Where(x => x.CustomerUserId == customerUserId)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id).ToListAsync(cancellationToken);

    public Task<CustomerAddress?> GetAddressAsync(int addressId, CancellationToken cancellationToken = default) =>
        context.CustomerAddresses.FirstOrDefaultAsync(x => x.Id == addressId, cancellationToken);

    public async Task AddAddressAsync(CustomerAddress address, CancellationToken cancellationToken = default) =>
        await context.CustomerAddresses.AddAsync(address, cancellationToken);
    public void UpdateAddress(CustomerAddress address) => context.CustomerAddresses.Update(address);
    public void DeleteAddress(CustomerAddress address) => context.CustomerAddresses.Remove(address);
}
