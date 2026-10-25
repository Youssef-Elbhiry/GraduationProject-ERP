using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence;

internal class ErpDbContext : DbContext
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
    {
        
    }
    // put Dbsets here
}
