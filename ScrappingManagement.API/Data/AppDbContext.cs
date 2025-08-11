using Microsoft.EntityFrameworkCore;
using ScrappingManagement.API.Models;

namespace ScrappingManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<QuoteProduct> QuoteProducts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Quote> Quotes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Quotes)
                .WithOne(t => t.Customer)
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // optional

            base.OnModelCreating(modelBuilder);
        }
    }

}
