
using ERP.Domain.Entities;
using ERP.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using ERP.Application.Interfaces;
using ERP.Application.Services;
using ERP.Infrastructure.Repositories;
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

        // register identity and identitystores

        service.AddIdentityCore<AppUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ErpDbContext>();
            //.AddDefaultTokenProviders();

        service.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        service.AddScoped<IUnitOfWork, UnitOfWork>();
        service.AddScoped<IFileService, FileService>();
        service.AddScoped<ICategoryService, CategoryService>();
        service.AddScoped<IProductService, ProductService>();
    }
}
