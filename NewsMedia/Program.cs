using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewsMedia.Data;
using NewsMedia.Models;
using NewsMedia.Repositories;
using NewsMedia.Business;
using NewsMedia.Business.FetchStrategies;
using NewsMedia.Api;
using NewsMedia.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Identity sigue manejando usuarios y contraseñas; JWT solo autentica las llamadas a la API
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Falta la configuración Jwt:Key.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddScoped<ISourceRepository, SourceRepository>();
builder.Services.AddScoped<ISourceItemRepository, SourceItemRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();

builder.Services.AddScoped<ISourceBusiness, SourceBusiness>();
builder.Services.AddScoped<ISourceItemBusiness, SourceItemBusiness>();
builder.Services.AddScoped<ISettingBusiness, SettingBusiness>();

// Patrón Strategy para el parseo de fuentes (RSS, XML, JSON, API, HTML)
builder.Services.AddTransient<IFetchStrategy, RssFetchStrategy>();
builder.Services.AddTransient<IFetchStrategy, XmlFetchStrategy>();
builder.Services.AddTransient<IFetchStrategy, JsonFetchStrategy>();
builder.Services.AddTransient<IFetchStrategy, ApiFetchStrategy>();
builder.Services.AddTransient<IFetchStrategy, HtmlFetchStrategy>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvc", policy =>
        policy.WithOrigins("https://localhost:7294", "http://localhost:5294")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

var app = builder.Build();

app.UseCors("AllowMvc");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();