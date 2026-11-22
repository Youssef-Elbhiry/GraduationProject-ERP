
using ERP.Application.Extensions;
using ERP.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ERP.Infrastructure.Persistence;
using ERP.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ERP.API;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddApplication(builder.Configuration);


        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("qwertyuioASD123@!sljdfvnaierjnvdmks")),
                ClockSkew = TimeSpan.Zero
            };
        });

        //builder.Services.AddCors(options =>
        //{
        //    options.AddPolicy("Mypolicy", policy => policy
        //    .WithOrigins("", "http://localhost:5173")
        //    .AllowAnyMethod()
        //    .AllowAnyHeader()
        //    .AllowCredentials());
        //});

        var app = builder.Build();

        await using (var scope = app.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ErpDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await DefaultDataSeeder.SeedRolesAsync(roleManager);
            await DefaultDataSeeder.SeedWarehouseAndFactoryAsync(context);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles();

        app.MapControllers();


        app.Run();
    }
}
