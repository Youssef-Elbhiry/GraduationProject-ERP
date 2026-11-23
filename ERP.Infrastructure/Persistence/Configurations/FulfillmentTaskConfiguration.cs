using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class FulfillmentTaskConfiguration : IEntityTypeConfiguration<FulfillmentTask>
{
    public void Configure(EntityTypeBuilder<FulfillmentTask> builder)
    {
        builder.ToTable("FulfillmentTasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().IsRequired().HasMaxLength(20);
        builder.HasOne(x => x.SalesOrder).WithOne(x => x.FulfillmentTask).HasForeignKey<FulfillmentTask>(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Warehouse).WithMany().HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.SalesOrderId).IsUnique();
        builder.HasIndex(x => x.WarehouseId);
    }
}
