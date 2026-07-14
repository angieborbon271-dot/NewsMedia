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
        public DbSet<Collection> Collections { get; set; }
        // Users ya lo provee IdentityDbContext automáticamente

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Una carpeta tiene muchos items; borrar la carpeta deja los items sin carpeta
            builder.Entity<Collection>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Collection)
                .HasForeignKey(i => i.CollectionId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}