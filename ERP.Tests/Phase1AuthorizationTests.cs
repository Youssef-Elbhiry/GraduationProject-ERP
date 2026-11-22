using System.Reflection;
using System.Security.Claims;
using ERP.API.Controllers;
using ERP.Application.Customer;
using ERP.Application.Customer.Dtos;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Tests;

public class Phase1AuthorizationTests
{
    [Fact]
    public async Task Customer_cannot_read_edit_or_delete_another_customers_address()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var service = new CustomerService(new UnitOfWork(fixture.Context));
        var controller = new CustomerController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = CreateCustomerContext("customer-a") }
        };
        var addressId = fixture.OtherCustomerAddressId;

        Assert.IsType<NotFoundObjectResult>(await controller.GetMyAddress(addressId, CancellationToken.None));
        Assert.IsType<NotFoundObjectResult>(await controller.UpdateMyAddress(addressId,
            new CustomerAddressWriteDto { Line1 = "Updated", City = "Cairo" }, CancellationToken.None));
        Assert.IsType<NotFoundObjectResult>(await controller.DeleteMyAddress(addressId, CancellationToken.None));

        var listResult = Assert.IsType<OkObjectResult>(await controller.GetMyAddresses(CancellationToken.None));
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyCollection<CustomerAddressDto>>(listResult.Value));
    }

    [Fact]
    public async Task Customer_calling_warehouse_staff_endpoint_is_forbidden()
    {
        using var services = new ServiceCollection().AddLogging().AddAuthorization().BuildServiceProvider();
        var httpContext = CreateCustomerContext("customer-a");
        httpContext.RequestServices = services;
        var roles = typeof(WarehouseController).GetCustomAttributes<AuthorizeAttribute>().Single().Roles!;
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(roles.Split(',')).Build();
        var authorizationService = services.GetRequiredService<IAuthorizationService>();
        var policyEvaluator = new PolicyEvaluator(authorizationService);
        var authenticateResult = await policyEvaluator.AuthenticateAsync(policy, httpContext);

        var result = await policyEvaluator.AuthorizeAsync(policy, authenticateResult, httpContext, resource: null);

        Assert.True(result.Forbidden, "The Customer role must be forbidden by the Warehouse endpoint policy (HTTP 403).");
    }

    private static DefaultHttpContext CreateCustomerContext(string customerUserId) => new()
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, customerUserId),
            new Claim(ClaimTypes.Role, "Customer")
        }, "Test"))
    };

    private sealed class DatabaseFixture(SqliteConnection connection, ErpDbContext context, int addressId) : IAsyncDisposable
    {
        public ErpDbContext Context { get; } = context;
        public int OtherCustomerAddressId { get; } = addressId;

        public static async Task<DatabaseFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ErpDbContext>().UseSqlite(connection).Options;
            var context = new ErpDbContext(options);
            await context.Database.EnsureCreatedAsync();
            context.Users.AddRange(
                new AppUser { Id = "customer-a", UserName = "a@example.com", Name = "A" },
                new AppUser { Id = "customer-b", UserName = "b@example.com", Name = "B" });
            var address = new CustomerAddress { CustomerUserId = "customer-b", Line1 = "Other address", City = "Cairo" };
            context.CustomerAddresses.Add(address);
            await context.SaveChangesAsync();
            return new DatabaseFixture(connection, context, address.Id);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
