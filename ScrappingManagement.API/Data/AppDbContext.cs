using Microsoft.EntityFrameworkCore;
using ScrappingManagement.API.Models;

namespace ScrappingManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<QuoteProduct> QuoteProducts { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<Product> Products { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
