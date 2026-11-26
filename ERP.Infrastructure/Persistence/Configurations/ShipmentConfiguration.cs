using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("Shipments");
        builder.HasKey(shipment => shipment.Id);

        builder.Property(shipment => shipment.ShipmentNo)
            .HasMaxLength(40)
            .IsRequired();
        builder.HasIndex(shipment => shipment.ShipmentNo)
            .IsUnique();
        builder.HasIndex(shipment => shipment.FulfillmentTaskId)
            .IsUnique();
        builder.Property(shipment => shipment.Carrier)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(shipment => shipment.TrackingNo)
            .HasMaxLength(100);
        builder.Property(shipment => shipment.ShippedByUserId)
            .HasMaxLength(450)
            .IsRequired();

        builder.HasOne(shipment => shipment.FulfillmentTask)
            .WithOne(task => task.Shipment)
            .HasForeignKey<Shipment>(shipment => shipment.FulfillmentTaskId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(shipment => shipment.ShippedByUser)
            .WithMany()
            .HasForeignKey(shipment => shipment.ShippedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
