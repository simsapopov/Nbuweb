using Microsoft.EntityFrameworkCore;
using NbuWeb.Models;

namespace NbuWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BankProduct> BankProducts { get; set; }
        public DbSet<InterestDetail> InterestDetails { get; set; }
        public DbSet<DepositDetail> DepositDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BankProduct>()
                .HasMany(p => p.InterestDetails)
                .WithOne(d => d.BankProduct)
                .HasForeignKey(d => d.BankProductId);

            modelBuilder.Entity<InterestDetail>()
                .HasOne(d => d.BankProduct)
                .WithMany(p => p.InterestDetails)
                .HasForeignKey(d => d.BankProductId);
        }
    }
}
