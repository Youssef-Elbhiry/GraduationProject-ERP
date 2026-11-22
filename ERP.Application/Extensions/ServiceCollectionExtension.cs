using ERP.Application.Auth;
using ERP.Application.Customer;
using ERP.Application.Warehouse;
using ERP.Application.Factory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ERP.Application.Extensions;

  public static class ServiceCollectionExtension
{
    public static void AddApplication(this IServiceCollection services, IConfiguration configuration)
    {

        var assembly = typeof(ServiceCollectionExtension).Assembly;
        services.AddAutoMapper(assembly);

        //services.AddValidatorsFromAssembly(assembly);

        //services.AddFluentValidationAutoValidation();
        //services.AddFluentValidationClientsideAdapters();

        services.AddScoped<IAuthService , AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IFactoryService, FactoryService>();

        services.Configure<JwtOptions>(configuration.GetSection("JWT"));
    }
}
