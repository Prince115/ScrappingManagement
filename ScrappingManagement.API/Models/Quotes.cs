using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.API.Models;
public class Quote
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string Location { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    [ForeignKey("CustomerId")]
    public Customer? Customer { get; set; }

    public ICollection<QuoteProduct> QuoteProducts { get; set; } = new List<QuoteProduct>();

    public double FinalTotal => QuoteProducts.Sum(e => e.TotalAmount);
}
