using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.Web.Models;
public class Quote
{
	public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty;  

	public DateTime Date { get; set; } = DateTime.UtcNow;
	public string Location { get; set; } = string.Empty;

	public int SupplierId { get; set; }
	[ForeignKey("SupplierId")]
	public Supplier? Supplier { get; set; }

	public ICollection<QuoteProduct> QuoteProducts { get; set; } = new List<QuoteProduct>();

	public decimal Total { get; set; }
	public decimal FinalTotal { get; set; }
	public decimal Kato { get; set; }
}
