using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Entities.User> Users { get; set; }
        public DbSet<Entities.UserAccess> UsersAccess { get; set; }

        public DataContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("site");

            modelBuilder.Entity<Entities.UserAccess>()
                .HasIndex(a => a.Login)
                .IsUnique();

            modelBuilder.Entity<Entities.UserAccess>()
                .HasOne(a => a.User)
                .WithMany(u => u.Accesses)
                .HasPrincipalKey(u => u.Id)
                .HasForeignKey(a => a.UserId);

            modelBuilder.Entity<Entities.User>()
                .HasIndex(u => u.Slug);
        }
    }
}
