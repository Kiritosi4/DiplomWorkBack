using DiplomWork.Persistance.JWT;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace DiplomWork.WebApi.Extensions
{
    public static class ApiExtensions
    {
        public static void AddAppAuthentication(this IServiceCollection services, IConfiguration config)
        {
            JwtOptions jwtOptions = config.GetSection(nameof(JwtOptions)).Get<JwtOptions>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["Auth"] ?? context.Request.Headers["Authorization"];
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
        }

        public static void MakeMigrations(this IApplicationBuilder builder)
        {
            using IServiceScope scope = builder.ApplicationServices.CreateScope();

            using DiplomWorkDbContext dbContext = scope.ServiceProvider.GetRequiredService<DiplomWorkDbContext>();


            if (dbContext.Database.GetPendingMigrations().Any())
            {
                dbContext.Database.Migrate();
            }

        }
    }
}
