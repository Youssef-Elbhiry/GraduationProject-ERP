using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    // put dependency here
    public static void AddInfrastructure(this IServiceCollection service 
        , IConfiguration configuration)
    {
        // Add dbcontext to dependency injection container
        service.AddDbContext<ErpDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("ConnectionString"))
        );
    }
}
