using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScrappingManagement.Web.Dto;

public class InvoiceViewModel
{
	public int Id { get; set; }
	public string InvoiceNumber { get; set; } = "";
	public DateOnly Date { get; set; }
	public int CustomerId { get; set; }
	public string Location { get; set; } = "";
	public string? BillNo { get; set; } = "";
	public string? BookNo { get; set; } = "";
	public string? Note { get; set; }
	public decimal PackagingCharge { get; set; }
	public bool WithGst { get; set; } = false;
	public decimal GstPercentage { get; set; } = 0m;
	public decimal GstValue { get; set; } = 0m;

	public decimal FinalAmount { get; set; }
	public decimal TcsPercentage { get; set; } = 0m;
	public decimal TcsValue { get; set; } = 0m;

	public List<InvoiceItemViewModel> Items { get; set; } = new();
	[ValidateNever]
	public List<SelectListItem> Customers { get; set; } = new();
	[ValidateNever]
	public List<SelectListItem> Products { get; set; } = new();
}

public class InvoiceItemViewModel
{
	public int Id { get; set; }
	public bool Deleted { get; set; } = false;
	public int ProductId { get; set; }
	public decimal Weight { get; set; }
	public decimal Rate { get; set; }
	public decimal Amount { get; set; }
}

public class InvoiceItems : InvoiceItemViewModel
{
	public string Name { get; set; }
}
