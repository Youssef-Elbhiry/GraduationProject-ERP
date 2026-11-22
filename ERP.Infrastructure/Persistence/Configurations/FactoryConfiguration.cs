using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class FactoryConfiguration : IEntityTypeConfiguration<Factory>
{
    public void Configure(EntityTypeBuilder<Factory> builder)
    {
        builder.ToTable("Factories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.HasOne(x => x.Warehouse).WithMany(x => x.Factories).HasForeignKey(x => x.WarehouseId).OnDelete(DeleteBehavior.Restrict);
    }
}
