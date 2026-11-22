using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ERP.Infrastructure.Persistence;

public class ErpDbContextFactory : IDesignTimeDbContextFactory<ErpDbContext>
{
    public ErpDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ErpDbContext>()
            .UseSqlServer("Server=.\\SQLEXPRESS;Database=Erp;Integrated Security=true;TrustServerCertificate=true;")
            .Options;
        return new ErpDbContext(options);
    }
}
