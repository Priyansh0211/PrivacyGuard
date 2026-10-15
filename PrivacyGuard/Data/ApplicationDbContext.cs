using Microsoft.EntityFrameworkCore;
using PrivacyGuard.Models;

namespace PrivacyGuard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Organization> Organizations { get; set; } 
        public DbSet<User> Users { get; set; }
        public DbSet<ConsentRecord> ConsentRecords { get; set; }
        public DbSet<PrivacyRequest> PrivacyRequests { get; set; }

        public DbSet<DataInventory> DataInventories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DataInventory>()
                .HasOne(d => d.Organization)
                .WithMany()
                .HasForeignKey(d => d.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}