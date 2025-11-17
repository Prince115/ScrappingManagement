using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
namespace ScrappingManagement.Web.Controllers;

[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
	private readonly AppDbContext _context;

	public HomeController(AppDbContext context)
	{
		_context = context;
	}

	public async Task<IActionResult> Dashboard(DateTime? startDate, DateTime? endDate)
	{
		var invoices = _context.Invoices.AsQueryable();
		var payments = _context.Receipts.AsQueryable();
		var quotes = _context.Quotes.AsQueryable();
		var customers = _context.Customers.AsQueryable();

		if (startDate.HasValue)
		{
			invoices = invoices.Where(i => i.Date >= startDate.Value);
			payments = payments.Where(p => p.Date >= startDate.Value);
			quotes = quotes.Where(q => q.Date >= startDate.Value);
		}

		if (endDate.HasValue)
		{
			invoices = invoices.Where(i => i.Date <= endDate.Value);
			payments = payments.Where(p => p.Date <= endDate.Value);
			quotes = quotes.Where(q => q.Date <= endDate.Value);
		}

		var vm = new DashboardViewModel
		{
			TotalCustomers = await customers.CountAsync(),
			TotalInvoices = await invoices.CountAsync(),
			TotalQuotes = await quotes.CountAsync(),
			TotalSuppliers = await _context.Suppliers.CountAsync(),
			TotalInvoiceAmount = await invoices.SumAsync(i => i.FinalAmount),
			TotalReceivedAmount = await payments.SumAsync(p => p.Amount),
			TotalDueAmount = await customers.SumAsync(s => s.OpeningBalance ?? 0) + await invoices.SumAsync(i => i.FinalAmount) - await payments.SumAsync(p => p.Amount),
			RecentInvoices = await invoices
				.OrderByDescending(i => i.Date)
				.Take(5)
				.Select(i => new RecentInvoiceDto
				{
					Id = i.Id,
					InvoiceNumber = i.InvoiceNumber,
					CustomerName = i.Customer.Name,
					Date = i.Date,
					FinalAmount = i.FinalAmount
				})
				.ToListAsync(),
			TopProducts = await _context.InvoiceItems
				.Where(ii => invoices.Select(i => i.Id).Contains(ii.InvoiceId))
				.GroupBy(ii => ii.Product.Name)
				.Select(g => new TopProductDto
				{
					ProductName = g.Key,
					TotalWeight = g.Sum(ii => ii.Weight),
					TotalRevenue = g.Sum(ii => ii.Amount)
				})
				.OrderByDescending(p => p.TotalRevenue)
				.Take(5)
				.ToListAsync(),
			StartDate = startDate,
			EndDate = endDate
		};

		return View(vm);
	}
}
