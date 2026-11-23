using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderNo).IsRequired().HasMaxLength(30);
        builder.HasIndex(x => x.OrderNo).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
        builder.Property(x => x.CreatedByUserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.CustomerUserId).IsRequired().HasMaxLength(450);

        builder.HasOne(x => x.CustomerUser).WithMany().HasForeignKey(x => x.CustomerUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ShippingAddress).WithMany().HasForeignKey(x => x.ShippingAddressId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
