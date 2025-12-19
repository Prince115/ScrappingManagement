using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Data
{
	public class AppDbContext : DbContext
	{
		private readonly IConfiguration _config;

		public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration config)
		    : base(options)
		{
			_config = config;
		}

		public DbSet<QuoteProduct> QuoteProducts { get; set; }
		public DbSet<Supplier> Suppliers { get; set; }
		public DbSet<Quote> Quotes { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Payment> Payments { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Invoice> Invoices { get; set; }
		public DbSet<InvoiceItem> InvoiceItems { get; set; }
		public DbSet<Receipt> Receipts { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Quote>()
		  .Property(e => e.Id)
		  .ValueGeneratedOnAdd();

			modelBuilder.Entity<QuoteProduct>()
			    .Property(e => e.Id)
			    .ValueGeneratedOnAdd();


			if (_config.GetValue<string>("Database") == "POSTGRESQL")
				modelBuilder.HasDefaultSchema("scpe");
		}
	}
}
