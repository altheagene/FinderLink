using Microsoft.EntityFrameworkCore;
using FinderLink.Models;

namespace FinderLink.Data
{
    public class FinderLinkDbContext : DbContext
    {
        public FinderLinkDbContext(DbContextOptions<FinderLinkDbContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Release> Releases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Admin relationships
            modelBuilder.Entity<Admin>()
                .HasMany(a => a.ItemsFound)
                .WithOne(i => i.FoundByAdmin)
                .HasForeignKey(i => i.FoundByAdminId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Admin>()
                .HasMany(a => a.ItemsCreated)
                .WithOne(i => i.CreatedByAdmin)
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Admin>()
                .HasMany(a => a.ClaimsVerified)
                .WithOne(c => c.VerifiedByAdmin)
                .HasForeignKey(c => c.VerifiedBy)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Admin>()
                .HasMany(a => a.AdminLogs)
                .WithOne(al => al.Admin)
                .HasForeignKey(al => al.AdminId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Username)
                .IsUnique();

            // Item relationships
            modelBuilder.Entity<Item>()
                .HasMany(i => i.Claims)
                .WithOne(c => c.Item)
                .HasForeignKey(c => c.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                .HasMany(i => i.Releases)
                .WithOne(r => r.Item)
                .HasForeignKey(r => r.ItemId)
                .OnDelete(DeleteBehavior.NoAction);

            // Claim relationships
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Admin)
                .WithMany()
                .HasForeignKey(c => c.AdminId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Release)
                .WithOne(r => r.Claim)
                .HasForeignKey<Release>(r => r.ClaimId)
                .OnDelete(DeleteBehavior.Restrict);

            // Release relationships
            modelBuilder.Entity<Release>()
                .HasOne(r => r.ReleasedToAdmin)
                .WithMany(a => a.ReceivedReleases)
                .HasForeignKey(r => r.ReleasedTo)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Release>()
                .HasOne(r => r.ReleasedByAdmin)
                .WithMany(a => a.ReleasedItems)
                .HasForeignKey(r => r.ReleasedBy)
                .OnDelete(DeleteBehavior.NoAction);

            // Indexes for common queries
            modelBuilder.Entity<Item>()
                .HasIndex(i => i.Status);

            modelBuilder.Entity<Item>()
                .HasIndex(i => i.DateFound);

            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.Status);

            modelBuilder.Entity<Claim>()
                .HasIndex(c => c.ItemId);

            modelBuilder.Entity<AdminLog>()
                .HasIndex(al => al.LogDate);
        }
    }
}
