using Microsoft.EntityFrameworkCore;
using DatingApp.Models;

namespace DatingApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AboutUser> AboutUsers { get; set; }
        public DbSet<UserLocation> UserLocations { get; set; }
        public DbSet<Preferences> Preferences  { get; set; }
        public DbSet<ProfilePhoto> ProfilePhotos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasOne(u => u.AboutUser)
            .WithOne()
            .HasForeignKey<AboutUser>(a => a.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.ProfilePhoto)
                .WithOne()
                .HasForeignKey<ProfilePhoto>(p => p.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserLocation)
                .WithOne()
                .HasForeignKey<UserLocation>(l => l.UserId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Preferences)
                .WithOne()
                .HasForeignKey<Preferences>(p => p.UserId);
        }
    }
}
