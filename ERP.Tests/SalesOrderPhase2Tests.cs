using System.Net.Mail;
using System.Reflection;
using System.Security.Claims;
using ERP.API.Controllers;
using ERP.Application.SalesOrder;
using ERP.Application.SalesOrder.Dtos;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ERP.Tests;

public class SalesOrderPhase2Tests
{
    [Fact]
    public async Task Customer_cannot_read_confirm_or_reject_another_customers_order()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var order = await fixture.CreateSentOrderAsync("customer-b", fixture.AddressB.Id);
        var controller = fixture.CreateController("customer-a", UserRole.Customer);

        Assert.IsType<NotFoundObjectResult>(await controller.GetMyOrder(order.Id, CancellationToken.None));
        Assert.IsType<NotFoundObjectResult>(await controller.Confirm(order.Id, CancellationToken.None));
        Assert.IsType<NotFoundObjectResult>(await controller.Reject(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Customer_calling_staff_sales_order_filter_is_forbidden()
    {
        using var services = new ServiceCollection().AddLogging().AddAuthorization().BuildServiceProvider();
        var method = typeof(SalesOrderController).GetMethod(nameof(SalesOrderController.GetAll))!;
        var attribute = method.GetCustomAttribute<AuthorizeAttribute>()!;
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().RequireRole(attribute.Roles!.Split(',')).Build();
        var context = new DefaultHttpContext
        {
            RequestServices = services,
            User = CreatePrincipal("customer-a", UserRole.Customer)
        };
        var evaluator = new PolicyEvaluator(services.GetRequiredService<IAuthorizationService>());
        var authentication = await evaluator.AuthenticateAsync(policy, context);

        var result = await evaluator.AuthorizeAsync(policy, authentication, context, resource: null);

        Assert.True(result.Forbidden);
    }

    [Fact]
    public async Task Create_with_an_address_owned_by_another_customer_returns_400()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var controller = fixture.CreateController("sales-user", UserRole.Sales);

        var response = await controller.Create(new SalesOrderWriteDto
        {
            CustomerUserId = "customer-a",
            ShippingAddressId = fixture.AddressB.Id,
            ValidUntil = DateTime.UtcNow.AddDays(5),
            Lines = new List<SalesOrderLineInputDto> { new() { ProductId = fixture.Product.Id, Qty = 2m } }
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(response);
    }

    [Fact]
    public async Task Confirming_twice_creates_only_one_fulfillment_task()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var order = await fixture.CreateSentOrderAsync("customer-a", fixture.AddressA.Id);
        var controller = fixture.CreateController("customer-a", UserRole.Customer);

        var first = Assert.IsType<OkObjectResult>(await controller.Confirm(order.Id, CancellationToken.None));
        var second = Assert.IsType<OkObjectResult>(await controller.Confirm(order.Id, CancellationToken.None));

        Assert.Equal("Confirmed", Assert.IsType<SalesOrderDto>(first.Value).Status);
        Assert.Equal("Confirmed", Assert.IsType<SalesOrderDto>(second.Value).Status);
        Assert.Equal(1, await fixture.Context.FulfillmentTasks.CountAsync(x => x.SalesOrderId == order.Id));
        Assert.Equal(FulfillmentStatus.Waiting,
            (await fixture.Context.FulfillmentTasks.SingleAsync(x => x.SalesOrderId == order.Id)).Status);
    }

    [Fact]
    public async Task Send_email_failure_keeps_order_sent_and_returns_emailSent_false()
    {
        await using var fixture = await DatabaseFixture.CreateAsync(new FailingEmailSender());
        var order = await fixture.CreateDraftOrderAsync("customer-a", fixture.AddressA.Id);
        var controller = fixture.CreateController("sales-user", UserRole.Sales);

        var response = Assert.IsType<OkObjectResult>(await controller.Send(order.Id, CancellationToken.None));

        var payload = Assert.IsType<SendSalesOrderResultDto>(response.Value);
        Assert.False(payload.EmailSent);
        Assert.Equal("Sent", payload.Order.Status);
        Assert.Equal(SalesOrderStatus.Sent, (await fixture.Context.SalesOrders.SingleAsync(x => x.Id == order.Id)).Status);
    }

    [Fact]
    public async Task Create_rejects_inactive_products_and_non_positive_quantities()
    {
        await using var fixture = await DatabaseFixture.CreateAsync();
        var controller = fixture.CreateController("sales-user", UserRole.Sales);
        var inactiveProduct = new Product
        {
            Code = "OFF", Name = "Inactive", SalesPrice = 1m, CostPrice = 0m, CategoryId = fixture.Category.Id,
            IsActive = false, ImageUrl = string.Empty
        };
        fixture.Context.Products.Add(inactiveProduct);
        await fixture.Context.SaveChangesAsync();

        var inactiveResponse = await controller.Create(new SalesOrderWriteDto
        {
            CustomerUserId = "customer-a", ShippingAddressId = fixture.AddressA.Id,
            ValidUntil = DateTime.UtcNow.AddDays(2),
            Lines = new List<SalesOrderLineInputDto> { new() { ProductId = inactiveProduct.Id, Qty = 1m } }
        }, CancellationToken.None);
        var quantityResponse = await controller.Create(new SalesOrderWriteDto
        {
            CustomerUserId = "customer-a", ShippingAddressId = fixture.AddressA.Id,
            ValidUntil = DateTime.UtcNow.AddDays(2),
            Lines = new List<SalesOrderLineInputDto> { new() { ProductId = fixture.Product.Id, Qty = 0m } }
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(inactiveResponse);
        Assert.IsType<BadRequestObjectResult>(quantityResponse);
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, string role) => new(new ClaimsIdentity(new[]
    {
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Role, role)
    }, "Test"));

    private sealed class FailingEmailSender : IEmailSender
    {
        public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default) =>
            Task.FromException(new SmtpException("Test SMTP failure"));
    }

    private sealed class TestDocumentNumberGenerator : IDocumentNumberGenerator
    {
        private int _next = 1;
        public Task<string> GenerateOrderNoAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult($"SO-2026-{_next++:D6}");
    }

    private sealed class DatabaseFixture(SqliteConnection connection, ErpDbContext context, IEmailSender emailSender) : IAsyncDisposable
    {
        public ErpDbContext Context { get; } = context;
        public CustomerAddress AddressA { get; private set; } = null!;
        public CustomerAddress AddressB { get; private set; } = null!;
        public Product Product { get; private set; } = null!;
        public Category Category { get; private set; } = null!;
        private UnitOfWork UnitOfWork { get; } = new(context);
        private SalesOrderService Service { get; } = new(new UnitOfWork(context), new TestDocumentNumberGenerator(), emailSender, NullLogger<SalesOrderService>.Instance);

        public static async Task<DatabaseFixture> CreateAsync(IEmailSender? emailSender = null)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<ErpDbContext>().UseSqlite(connection).Options;
            var context = new ErpDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var customerRole = new IdentityRole(UserRole.Customer);
            var salesRole = new IdentityRole(UserRole.Sales);
            var customerA = new AppUser { Id = "customer-a", UserName = "a@example.com", Email = "a@example.com", Name = "Customer A" };
            var customerB = new AppUser { Id = "customer-b", UserName = "b@example.com", Email = "b@example.com", Name = "Customer B" };
            var sales = new AppUser { Id = "sales-user", UserName = "sales@example.com", Email = "sales@example.com", Name = "Sales" };
            context.Roles.AddRange(customerRole, salesRole);
            context.Users.AddRange(customerA, customerB, sales);
            context.UserRoles.AddRange(
                new IdentityUserRole<string> { UserId = customerA.Id, RoleId = customerRole.Id },
                new IdentityUserRole<string> { UserId = customerB.Id, RoleId = customerRole.Id },
                new IdentityUserRole<string> { UserId = sales.Id, RoleId = salesRole.Id });
            var addressA = new CustomerAddress { CustomerUserId = customerA.Id, Line1 = "A Road", City = "Cairo", IsDefault = true };
            var addressB = new CustomerAddress { CustomerUserId = customerB.Id, Line1 = "B Road", City = "Cairo", IsDefault = true };
            var category = new Category { Name = "Test" };
            context.CustomerAddresses.AddRange(addressA, addressB);
            context.Categories.Add(category);
            var warehouse = new Warehouse { Name = "Default", IsActive = true };
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();
            var product = new Product
            {
                Code = "TEST", Name = "Test product", SalesPrice = 12.50m, CostPrice = 5m,
                CategoryId = category.Id, IsActive = true, ImageUrl = string.Empty
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            return new DatabaseFixture(connection, context, emailSender ?? new NoOpEmailSender())
            {
                AddressA = addressA,
                AddressB = addressB,
                Product = product,
                Category = category
            };
        }

        public SalesOrderController CreateController(string userId, string role) => new(Service)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = CreatePrincipal(userId, role) } }
        };

        public async Task<SalesOrder> CreateSentOrderAsync(string customerId, int addressId)
        {
            var order = NewOrder(customerId, addressId, SalesOrderStatus.Sent);
            Context.SalesOrders.Add(order);
            await Context.SaveChangesAsync();
            return order;
        }

        public async Task<SalesOrder> CreateDraftOrderAsync(string customerId, int addressId)
        {
            var order = NewOrder(customerId, addressId, SalesOrderStatus.Draft);
            Context.SalesOrders.Add(order);
            await Context.SaveChangesAsync();
            return order;
        }

        private SalesOrder NewOrder(string customerId, int addressId, SalesOrderStatus status) => new()
        {
            OrderNo = $"SO-2026-{Guid.NewGuid():N}"[..16],
            CustomerUserId = customerId,
            ShippingAddressId = addressId,
            CreatedByUserId = "sales-user",
            CreatedAt = DateTime.UtcNow,
            ValidUntil = DateTime.UtcNow.AddDays(3),
            SentAt = status == SalesOrderStatus.Sent ? DateTime.UtcNow : null,
            Status = status,
            Lines = new List<SalesOrderLine> { new() { ProductId = Product.Id, Qty = 2m, UnitPrice = Product.SalesPrice } }
        };

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class NoOpEmailSender : IEmailSender
    {
        public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
