namespace ScrappingManagement.Web.Models;
public class Invoice
{
	public int Id { get; set; }
	public string InvoiceNumber { get; set; }
	public DateTime Date { get; set; }
	public string Location { get; set; }
	public string? Note { get; set; }
	public decimal PackagingCharge { get; set; }
	public decimal FinalAmount { get; set; }

	public int CustomerId { get; set; }
	public Customer Customer { get; set; }

	public List<InvoiceItem>? Items { get; set; }
}
