using ERP.Application.ProductStock;
using ERP.Application.ProductStock.Dtos;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using ERP.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ERP.Tests;

public class Phase3Tests
{
    [Fact]
    public async Task Waiting_task_cannot_ship_and_does_not_change_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var task = await fixture.AddTaskAsync(2m);
        var service = new ShipmentService(fixture.UnitOfWork);

        var result = await service.CreateAsync(
            new ShipmentCreateDto(task.Id, "Carrier", null),
            fixture.User.Id);

        Assert.Equal(409, result.StatusCode);
        Assert.Null(await fixture.UnitOfWork.Shipments.GetByTaskIdAsync(task.Id));

        var stock = await fixture.UnitOfWork.ProductStocks.GetAsync(
            fixture.Product.Id,
            fixture.Warehouse.Id);
        Assert.Equal(10m, stock!.OnHandQty);
    }

    [Fact]
    public async Task Availability_is_all_or_nothing_for_multiple_lines()
    {
        await using var fixture = await Fixture.CreateAsync();
        var secondProduct = await fixture.AddProductAsync();
        var task = await fixture.AddTaskAsync(2m, (secondProduct.Id, 3m));
        var service = new ProductStockService(fixture.UnitOfWork);

        var result = await service.CheckAvailabilityAsync(task.Id);

        Assert.Equal("Waiting", result.Value!.Status);
        Assert.Contains(
            result.Value.Lines,
            line => line.ProductId == secondProduct.Id && line.MissingQty == 3m);
    }

    [Fact]
    public async Task Competing_tasks_cannot_both_reserve_same_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var firstTask = await fixture.AddTaskAsync(7m);
        var secondTask = await fixture.AddTaskAsync(7m);
        var service = new ProductStockService(fixture.UnitOfWork);

        var firstResult = await service.CheckAvailabilityAsync(firstTask.Id);
        var secondResult = await service.CheckAvailabilityAsync(secondTask.Id);

        Assert.Equal("Ready", firstResult.Value!.Status);
        Assert.Equal("Waiting", secondResult.Value!.Status);
    }

    [Fact]
    public async Task Stock_in_rechecks_waiting_tasks_oldest_first()
    {
        await using var fixture = await Fixture.CreateAsync();
        var olderTask = await fixture.AddTaskAsync(13m);
        var newerTask = await fixture.AddTaskAsync(13m);
        var service = new ProductStockService(fixture.UnitOfWork);

        var result = await service.AdjustAsync(
            new StockAdjustmentDto(
                fixture.Product.Id,
                fixture.Warehouse.Id,
                14m,
                ProductMovementType.StockIn,
                null),
            fixture.User.Id);

        Assert.True(result.Succeeded);

        var olderTaskAfterAdjustment = await fixture.UnitOfWork.FulfillmentTasks
            .GetByIdAsync(olderTask.Id);
        var newerTaskAfterAdjustment = await fixture.UnitOfWork.FulfillmentTasks
            .GetByIdAsync(newerTask.Id);

        Assert.Equal(FulfillmentStatus.Ready, olderTaskAfterAdjustment!.Status);
        Assert.Equal(FulfillmentStatus.Waiting, newerTaskAfterAdjustment!.Status);
    }

    [Fact]
    public async Task Adjustment_rejects_negative_on_hand_stock()
    {
        await using var fixture = await Fixture.CreateAsync();
        var service = new ProductStockService(fixture.UnitOfWork);

        var result = await service.AdjustAsync(
            new StockAdjustmentDto(
                fixture.Product.Id,
                fixture.Warehouse.Id,
                -11m,
                ProductMovementType.Adjustment,
                null),
            fixture.User.Id);

        Assert.Equal(400, result.StatusCode);

        var stock = await fixture.UnitOfWork.ProductStocks.GetAsync(
            fixture.Product.Id,
            fixture.Warehouse.Id);
        Assert.Equal(10m, stock!.OnHandQty);
    }

    [Fact]
    public async Task Stale_stock_row_returns_409()
    {
        await using var fixture = await Fixture.CreateAsync();
        _ = await fixture.UnitOfWork.ProductStocks.GetAsync(
            fixture.Product.Id,
            fixture.Warehouse.Id);

        await fixture.Context.Database.ExecuteSqlRawAsync(
            "UPDATE ProductStocks SET OnHandQty = 9, "
            + "RowVersion = X'02000000000000000000000000000000'");

        var service = new ProductStockService(fixture.UnitOfWork);
        var result = await service.AdjustAsync(
            new StockAdjustmentDto(
                fixture.Product.Id,
                fixture.Warehouse.Id,
                1m,
                ProductMovementType.StockIn,
                null),
            fixture.User.Id);

        Assert.Equal(409, result.StatusCode);
        Assert.Equal("Stock was changed by someone else, please retry", result.Error);
    }

    private sealed class Fixture(
        SqliteConnection connection,
        ErpDbContext context) : IAsyncDisposable
    {
        public ErpDbContext Context { get; } = context;
        public UnitOfWork UnitOfWork { get; } = new(context);
        public AppUser User { get; private set; } = null!;
        public Product Product { get; private set; } = null!;
        public Warehouse Warehouse { get; private set; } = null!;
        private Category Category { get; set; } = null!;

        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ErpDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new ErpDbContext(options);
            await context.Database.EnsureCreatedAsync();

            var user = new AppUser
            {
                Id = "warehouse-user",
                UserName = "warehouse",
                Email = "w@example.com",
                Name = "Warehouse"
            };
            var category = new Category
            {
                Name = "Test"
            };
            var warehouse = new Warehouse
            {
                Name = "Main"
            };

            context.Users.Add(user);
            context.Categories.Add(category);
            context.Warehouses.Add(warehouse);
            await context.SaveChangesAsync();

            var product = new Product
            {
                Code = "P1",
                Name = "Product",
                CategoryId = category.Id,
                SalesPrice = 1m,
                CostPrice = 1m,
                ImageUrl = string.Empty,
                IsActive = true
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var stock = new ProductStock
            {
                ProductId = product.Id,
                WarehouseId = warehouse.Id,
                OnHandQty = 10m,
                RowVersion = [1]
            };
            context.ProductStocks.Add(stock);
            await context.SaveChangesAsync();

            return new Fixture(connection, context)
            {
                User = user,
                Category = category,
                Product = product,
                Warehouse = warehouse
            };
        }

        public async Task<Product> AddProductAsync()
        {
            var product = new Product
            {
                Code = Guid.NewGuid().ToString("N")[..8],
                Name = "Other",
                CategoryId = Category.Id,
                SalesPrice = 1m,
                CostPrice = 1m,
                ImageUrl = string.Empty,
                IsActive = true
            };

            Context.Products.Add(product);
            await Context.SaveChangesAsync();
            return product;
        }

        public async Task<FulfillmentTask> AddTaskAsync(
            decimal quantity,
            params (int ProductId, decimal Qty)[] additionalLines)
        {
            var address = new CustomerAddress
            {
                CustomerUserId = User.Id,
                Line1 = "1 Road",
                City = "Cairo"
            };

            Context.CustomerAddresses.Add(address);
            await Context.SaveChangesAsync();

            var order = new SalesOrder
            {
                OrderNo = Guid.NewGuid().ToString("N")[..20],
                CustomerUserId = User.Id,
                CreatedByUserId = User.Id,
                ShippingAddressId = address.Id,
                CreatedAt = DateTime.UtcNow,
                ValidUntil = DateTime.UtcNow.AddDays(2),
                Status = SalesOrderStatus.Confirmed
            };

            order.Lines.Add(new SalesOrderLine
            {
                ProductId = Product.Id,
                Qty = quantity,
                UnitPrice = 1m
            });

            foreach (var (productId, lineQuantity) in additionalLines)
            {
                order.Lines.Add(new SalesOrderLine
                {
                    ProductId = productId,
                    Qty = lineQuantity,
                    UnitPrice = 1m
                });
            }

            var task = new FulfillmentTask
            {
                SalesOrder = order,
                WarehouseId = Warehouse.Id,
                Status = FulfillmentStatus.Waiting,
                CreatedAt = DateTime.UtcNow
            };

            Context.FulfillmentTasks.Add(task);
            await Context.SaveChangesAsync();
            return task;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
