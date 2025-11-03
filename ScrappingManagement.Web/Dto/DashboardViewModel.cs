namespace ScrappingManagement.Web.Dto;

public class DashboardViewModel
{
	public int TotalSuppliers { get; set; }
	public int TotalCustomers { get; set; }
	public int TotalInvoices { get; set; }
	public int TotalQuotes { get; set; }
	public decimal TotalInvoiceAmount { get; set; }
	public decimal TotalReceivedAmount { get; set; }
	public decimal TotalDueAmount { get; set; }

	public DateTime? StartDate { get; set; }
	public DateTime? EndDate { get; set; }

	public List<RecentInvoiceDto> RecentInvoices { get; set; } = new List<RecentInvoiceDto>();
	public List<TopProductDto> TopProducts { get; set; } = new List<TopProductDto>();
}

public class RecentInvoiceDto
{
	public int Id { get; set; }
	public string InvoiceNumber { get; set; }
	public string CustomerName { get; set; }
	public DateTime Date { get; set; }
	public decimal FinalAmount { get; set; }
}

public class TopProductDto
{
	public string ProductName { get; set; }
	public decimal TotalWeight { get; set; }
	public decimal TotalRevenue { get; set; }
}
