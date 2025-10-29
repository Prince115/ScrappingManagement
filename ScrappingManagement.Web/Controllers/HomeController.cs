using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
namespace ScrappingManagement.Web.Controllers;

public class HomeController : Controller
{
	private readonly AppDbContext _context;

	public HomeController(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IActionResult> Dashboard()
	{
		var vm = new DashboardViewModel
		{
			TotalCustomers = await _context.Customers.CountAsync(),
			TotalInvoices = await _context.Invoices.CountAsync(),
			TotalQuotes = await _context.Quotes.CountAsync(),
			TotalSuppliers = await _context.Suppliers.CountAsync(),
		};

		return View(vm);
	}
}
