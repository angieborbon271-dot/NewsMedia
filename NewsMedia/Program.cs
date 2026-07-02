using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;
using NewsMedia.Repositories;
using NewsMedia.Business;

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

builder.Services.AddScoped<SourceRepository>();
builder.Services.AddScoped<SourceItemRepository>();
builder.Services.AddScoped<SettingRepository>();

builder.Services.AddScoped<SourceBusiness>();
builder.Services.AddScoped<SourceItemBusiness>();
builder.Services.AddScoped<SettingBusiness>();

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

app.Run();