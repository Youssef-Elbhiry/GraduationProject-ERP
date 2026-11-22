using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class CustomerAddressConfiguration : IEntityTypeConfiguration<CustomerAddress>
{
    public void Configure(EntityTypeBuilder<CustomerAddress> builder)
    {
        builder.ToTable("CustomerAddresses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CustomerUserId).IsRequired();
        builder.Property(x => x.Line1).IsRequired().HasMaxLength(250);
        builder.Property(x => x.Line2).HasMaxLength(250);
        builder.Property(x => x.City).IsRequired().HasMaxLength(100);
        builder.HasOne(x => x.CustomerUser).WithMany().HasForeignKey(x => x.CustomerUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.CustomerUserId).IsUnique().HasFilter("[IsDefault] = 1");
    }
}
