namespace ScrappingManagement.Web.Models;

public class Invoice
{
	public int Id { get; set; }
	public required string InvoiceNumber { get; set; }
	public string? BillNo { get; set; }
	public string? BookNo { get; set; }
	public DateOnly Date { get; set; }
	public string Location { get; set; } = "";
	public string? Note { get; set; }
	public decimal PackagingCharge { get; set; }
	public decimal FinalAmount { get; set; }
	public int CustomerId { get; set; }
	public Customer? Customer { get; set; }
	public List<InvoiceItem>? Items { get; set; }
	public bool WithGst { get; set; } = false;
	public decimal GstPercentage { get; set; } = 0m;
	public decimal GstValue { get; set; } = 0m;

	public decimal TcsPercentage { get; set; } = 0m;
	public decimal TcsValue { get; set; } = 0m;
}
