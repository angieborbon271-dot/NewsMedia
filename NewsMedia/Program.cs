using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;
using NewsMedia.Repositories;
using NewsMedia.Business;
using NewsMedia.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<ISourceRepository, SourceRepository>();
builder.Services.AddScoped<ISourceItemRepository, SourceItemRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();

builder.Services.AddScoped<ISourceBusiness, SourceBusiness>();
builder.Services.AddScoped<ISourceItemBusiness, SourceItemBusiness>();
builder.Services.AddScoped<ISettingBusiness, SettingBusiness>();

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