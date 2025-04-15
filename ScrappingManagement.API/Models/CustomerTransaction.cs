using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.API.Models
{
    public class CustomerTransaction
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Location { get; set; } = string.Empty;

        // Foreign key to Customer
        public int CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        // Related product entries
        public ICollection<ProductEntry> ProductEntries { get; set; } = new List<ProductEntry>();

        // Auto-calculated final total
        public double FinalTotal => ProductEntries.Sum(e => e.TotalAmount);
    }
}
