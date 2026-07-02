using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NewsMedia.Models;

namespace NewsMedia.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Source> Sources { get; set; }
        public DbSet<SourceItem> SourceItems { get; set; }
        public DbSet<Setting> Settings { get; set; }
        // Users ya lo provee IdentityDbContext automáticamente
    }
}