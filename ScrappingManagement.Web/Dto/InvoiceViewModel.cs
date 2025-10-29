using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScrappingManagement.Web.Dto;

public class InvoiceViewModel
{
	public int Id { get; set; }
	public string InvoiceNumber { get; set; }
	public DateTime Date { get; set; }
	public int CustomerId { get; set; }
	public string Location { get; set; }
	public string? Note { get; set; }
	public decimal PackagingCharge { get; set; }

	public List<InvoiceItemViewModel> Items { get; set; }
	[ValidateNever]
	public List<SelectListItem> Customers { get; set; }
	[ValidateNever]
	public List<SelectListItem> Products { get; set; }
}

public class InvoiceItemViewModel
{
	public int ProductId { get; set; }
	public decimal Weight { get; set; }
	public decimal Rate { get; set; }
	public decimal Amount { get; set; }
}
