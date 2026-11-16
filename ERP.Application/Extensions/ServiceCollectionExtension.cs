using ERP.Application.Auth;
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

        services.Configure<JwtOptions>(configuration.GetSection("JWT"));
    }
}
