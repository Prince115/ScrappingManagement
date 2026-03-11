using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers
{
	[Authorize(Roles = "Admin,User")]
	public class ReportController : Controller
	{
		private readonly AppDbContext _context;

		public ReportController(AppDbContext context)
		{
			_context = context;
		}

		[HttpGet("CustomerItems")]
		[Route("CustomerItems")]
		public async Task<IActionResult> CustomerItems(
		  int? selectedCustomerId,
		  DateOnly? fromDate,
		  DateOnly? toDate)
		{
			ViewBag.Customers = await _context.Customers.OrderBy(s => s.Name).ToListAsync();

			var model = new CustomerInvoiceWithItems
			{
				CustomerId = selectedCustomerId,
				FromDate = fromDate,
				ToDate = toDate
			};

			if (selectedCustomerId.HasValue)
			{
				var customer = await _context.Customers.FindAsync(selectedCustomerId.Value);

				model.CustomerName = customer?.Name;
				model.OpeningBalance = customer?.OpeningBalance ?? 0;

				var Invoices = _context.Invoices
				    .Where(q => q.CustomerId == selectedCustomerId.Value)
				    .Include(q => q.Items)
				    .Select(q => new LedgerEntryDto
				    {
					    Date = q.Date,
					    Type = "Invoice",
					    Description = q.Id.ToString(),
					    Debit = q.FinalAmount,
					    Credit = 0,
					    DocumentId = q.Id,
					    IsWithGst = q.WithGst,
					    BillNo = (q.BookNo ?? "-") + " / " + (q.BillNo ?? "-"),
					    Items = q.Items!.Select(i => new InvoiceItems
					    {
						    Name = i.Product.Name,
						    Weight = i.Weight,
						    Rate = i.Rate,
						    Amount = i.Amount
					    }).ToList()
				    });

				var Receipts = _context.Receipts
				    .Where(p => p.CustomerId == selectedCustomerId.Value)
				    .Select(p => new LedgerEntryDto
				    {
					    Date = p.Date,
					    Type = "Receipt",
					    Description = p.PaymentMode.ToString(),
					    Debit = 0,
					    Credit = p.Amount,
					    DocumentId = p.Id,
					    IsWithGst = p.WithGst
				    });

				if (fromDate.HasValue)
				{
					Invoices = Invoices.Where(q => q.Date >= fromDate.Value);
					Receipts = Receipts.Where(p => p.Date >= fromDate.Value);
				}
				if (toDate.HasValue)
				{
					Invoices = Invoices.Where(q => q.Date <= toDate.Value);
					Receipts = Receipts.Where(p => p.Date <= toDate.Value);
				}

				model.LedgerEntries.AddRange(await Invoices.ToListAsync());
				model.LedgerEntries.AddRange(await Receipts.ToListAsync());

				model.LedgerEntries = [.. model.LedgerEntries.OrderBy(e => e.Date)];

				model.TotalDebit = model.OpeningBalance + model.LedgerEntries.Where(e => e.Type == "Invoice").Sum(e => e.Debit);
				model.TotalCredit = model.LedgerEntries.Where(e => e.Type == "Receipt").Sum(e => e.Credit);
				model.DueAmount = model.TotalDebit - model.TotalCredit;
			}

			return View(model);
		}
	}
}
