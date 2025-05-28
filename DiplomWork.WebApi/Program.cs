using DiplomWork.Application.Services;
using DiplomWork.Models;
using Microsoft.AspNetCore.CookiePolicy;
using DiplomWork.WebApi.Extensions;
using Microsoft.EntityFrameworkCore;
using DiplomWork.Persistance;
using DiplomWork.Persistance.JWT;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Config
services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

// Services
services.AddScoped<JwtProvider>();
services.AddScoped<AuthService>();
services.AddScoped<ProfitService>();
services.AddScoped<ExpensesService>();
services.AddScoped<ExpenseCategoryService>();
services.AddScoped<ProfitCategoryService>();
services.AddScoped<BudgetService>();
services.AddScoped<TargetService>();
services.AddScoped<UserService>();


services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddAppAuthentication(configuration);

services.AddDbContext<DiplomWorkDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString(nameof(DiplomWorkDbContext)));
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.Always
});

app.UseCors(x =>
{
    x.WithHeaders().AllowAnyHeader();
    x.WithOrigins( "http://localhost:443", "https://localhost:443", "http://localhost:4173", "https://localhost:4173", "http://localhost:5173", "https://localhost:5173");
    x.WithMethods().AllowAnyMethod();
    x.AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MakeMigrations();

app.Run();
