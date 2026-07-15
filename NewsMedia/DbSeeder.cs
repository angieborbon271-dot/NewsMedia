using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsMedia.Data;
using NewsMedia.Models;

namespace NewsMedia.Api
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var context = services.GetRequiredService<AppDbContext>();

            await SeedRolesAsync(roleManager);
            await SeedAdminAsync(userManager);
            await SeedSourcesAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "admin", "user", "viewer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private static async Task SeedAdminAsync(UserManager<AppUser> userManager)
        {
            const string email = "admin@newsmedia.com";
            const string password = "Admin123!";

            if (await userManager.FindByEmailAsync(email) != null)
                return;

            var admin = new AppUser
            {
                UserName = email,
                Email = email,
                FirstName = "Admin",
                LastName = "NewsMedia",
                Role = "admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);

            if (!result.Succeeded)
            {
                Console.WriteLine("Error al crear el usuario admin:");
                foreach (var error in result.Errors)
                    Console.WriteLine($"  [{error.Code}] {error.Description}");
                return;
            }

            await userManager.AddToRoleAsync(admin, "admin");
        }

        private static async Task SeedSourcesAsync(AppDbContext context)
        {
            if (await context.Sources.AnyAsync())
                return;

            context.Sources.AddRange(
                new Source
                {
                    Name = "BBC Mundo",
                    Url = "https://feeds.bbci.co.uk/mundo/rss.xml",
                    ComponentType = "rss",
                    Description = "Noticias internacionales en español de la BBC.",
                    RequiresSecret = false
                },
                new Source
                {
                    Name = "El País",
                    Url = "https://elpais.com/rss/elpais/portada.xml",
                    ComponentType = "rss",
                    Description = "Portada del diario español El País.",
                    RequiresSecret = false
                },
                new Source
                {
                    Name = "Infobae",
                    Url = "https://www.infobae.com/arc/outboundfeeds/rss/",
                    ComponentType = "rss",
                    Description = "Últimas noticias de Infobae, medio argentino.",
                    RequiresSecret = false
                },
                new Source
                {
                    Name = "NY Times World",
                    Url = "https://rss.nytimes.com/services/xml/rss/nyt/World.xml",
                    ComponentType = "rss",
                    Description = "Sección internacional del diario The New York Times.",
                    RequiresSecret = false
                });

            await context.SaveChangesAsync();
        }
    }
}
