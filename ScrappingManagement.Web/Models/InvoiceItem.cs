namespace ScrappingManagement.Web.Models;
public class InvoiceItem
{
	public int Id { get; set; }
	public int InvoiceId { get; set; }
	public Invoice Invoice { get; set; }

	public int ProductId { get; set; }
	public Product Product { get; set; }

	public decimal Weight { get; set; }
	public decimal Rate { get; set; }
	public decimal Amount { get; set; }
}
