using ERP.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence;
public class ErpDbContext : IdentityDbContext<AppUser>
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
    {

    }
    // put Dbsets here
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CustomerAddress> CustomerAddresses { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Factory> Factories { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
    public DbSet<FulfillmentTask> FulfillmentTasks { get; set; }
    public DbSet<ProductStock> ProductStocks { get; set; }
    public DbSet<ProductStockMovement> ProductStockMovements { get; set; }
    public DbSet<Shipment> Shipments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Picks up every IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ErpDbContext).Assembly);
    }
}
