using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ProductStockMovementConfiguration
    : IEntityTypeConfiguration<ProductStockMovement>
{
    public void Configure(EntityTypeBuilder<ProductStockMovement> builder)
    {
        builder.ToTable("ProductStockMovements");
        builder.HasKey(movement => movement.Id);

        builder.Property(movement => movement.Type)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(movement => movement.QtyChange)
            .HasPrecision(18, 3);
        builder.Property(movement => movement.UserId)
            .HasMaxLength(450)
            .IsRequired();
        builder.Property(movement => movement.Note)
            .HasMaxLength(500);

        builder.HasOne(movement => movement.Product)
            .WithMany()
            .HasForeignKey(movement => movement.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(movement => movement.Warehouse)
            .WithMany()
            .HasForeignKey(movement => movement.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(movement => movement.User)
            .WithMany()
            .HasForeignKey(movement => movement.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(movement => movement.Shipment)
            .WithMany(shipment => shipment.Movements)
            .HasForeignKey(movement => movement.ShipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
