using ERP.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Persistence;

internal class ErpDbContext : IdentityDbContext<AppUser>
{
    public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
    {
        
    }
    // put Dbsets here
}
