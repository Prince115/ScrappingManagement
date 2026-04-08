using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.Web.Models;

public class Quote
{
	public int Id { get; set; }
	[Column("billnumber")]
	public string BillNumber { get; set; } = string.Empty;

	public DateOnly Date { get; set; }
	public string Location { get; set; } = string.Empty;

	public int SupplierId { get; set; }
	[ForeignKey("SupplierId")]
	public Supplier? Supplier { get; set; }

	public ICollection<QuoteProduct> QuoteProducts { get; set; } = [];

	public decimal Total { get; set; }
	public decimal FinalTotal { get; set; }

	[Column("kato")]
	public decimal Kato { get; set; }
	public string? Note { get; set; }
    public QuoteStatus Status { get; set; } = QuoteStatus.ReportPending;

    [Column("paymentamount")]
    public decimal? PaymentAmount { get; set; }

}
