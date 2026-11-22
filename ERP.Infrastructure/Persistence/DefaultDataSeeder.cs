using ERP.Domain.Entities;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence;

public static class DefaultDataSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[]
        {
            UserRole.Sales,
            UserRole.Customer,
            UserRole.Warehouse,
            UserRole.Accountant,
            UserRole.Admin
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role)) continue;

            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not seed role '{role}': {errors}");
            }
        }
    }

    public static async Task SeedWarehouseAndFactoryAsync(ErpDbContext context, CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        var warehouse = await context.Warehouses.OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        if (warehouse is null)
        {
            warehouse = new Warehouse { Name = "Default Warehouse", IsActive = true };
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Factories.AnyAsync(cancellationToken))
        {
            context.Factories.Add(new Factory
            {
                Name = "Default Factory", WarehouseId = warehouse.Id, IsActive = true
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
