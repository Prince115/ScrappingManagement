using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<QuoteProduct> QuoteProducts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
