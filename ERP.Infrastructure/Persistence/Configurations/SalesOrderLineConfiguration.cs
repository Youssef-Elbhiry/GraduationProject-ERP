using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Qty).HasColumnType("decimal(18,3)");
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.HasOne(x => x.SalesOrder).WithMany(x => x.Lines).HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.SalesOrderId);
        builder.HasIndex(x => x.ProductId);
    }
}
