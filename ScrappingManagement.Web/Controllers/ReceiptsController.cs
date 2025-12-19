using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Helpers;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers
{
	[Authorize(Roles = "Admin,User")]
	public class ReceiptsController(AppDbContext context) : Controller
	{
		private readonly AppDbContext _context = context;

		public async Task<IActionResult> Index(
		    int? pageNumber,
		    int? pageSize,
		    int? CustomerId,
		    PaymentMode? paymentMode,
		    DateOnly? fromDate,
		    DateOnly? toDate)
		{
			int currentPageSize = pageSize ?? 20;

			var receipts = _context.Receipts.AsQueryable();

			// Apply filters
			if (CustomerId.HasValue)
			{
				receipts = receipts.Where(p => p.CustomerId == CustomerId.Value);
			}

			if (paymentMode.HasValue)
			{
				receipts = receipts.Where(p => p.PaymentMode == paymentMode.Value);
			}

			if (fromDate.HasValue)
			{
				receipts = receipts.Where(p => p.Date >= fromDate.Value);
			}

			if (toDate.HasValue)
			{
				receipts = receipts.Where(p => p.Date <= toDate.Value);
			}

			var receiptDetailsQuery = receipts
				.Join(
					_context.Customers,
					payment => payment.CustomerId,
					customer => customer.Id,
					(payment, customer) => new ReceiptDetailDto
					{
						Id = payment.Id,
						CustomerId = payment.CustomerId,
						CustomerName = customer.Name,
						PaymentMode = payment.PaymentMode,
						WithGst = payment.WithGst,
						Amount = payment.Amount,
						Description = payment.Description ?? "",
						Date = payment.Date
					}
				)
				.OrderByDescending(p => p.Id)
				.AsNoTracking();

			var paged = await PaginatedList<ReceiptDetailDto>.CreateAsync(receiptDetailsQuery, pageNumber ?? 1, currentPageSize);

			var vm = new ReceiptIndexViewModel
			{
				Receipts = paged,
				Customers = await _context.Customers.OrderBy(s => s.Name).ToListAsync(),
				PaymentModes = new SelectList(Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>()),
				CurrentCustomerFilter = CustomerId,
				CurrentPaymentModeFilter = paymentMode,
				CurrentFromDateFilter = fromDate?.ToString("yyyy-MM-dd"),
				CurrentToDateFilter = toDate?.ToString("yyyy-MM-dd"),
				CurrentPageSize = currentPageSize,
				PageNumber = pageNumber ?? 1
			};

			return View(vm);
		}

		public async Task<IActionResult> Create()
		{
			ViewBag.Invoices = await _context.Invoices
			    .Include(i => i.Customer)
			    .OrderByDescending(i => i.Date)
			    .Select(i => new { i.Id, Display = i.InvoiceNumber + " - " + (i.Customer != null ? i.Customer.Name : "") })
			    .ToListAsync();
			ViewBag.Customers = await _context.Customers.ToListAsync();
			var model = new Receipt { Date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()) };
			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Receipt receipt)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.Invoices = await _context.Invoices
				    .Include(i => i.Customer)
				    .OrderByDescending(i => i.Date)
				    .Select(i => new { i.Id, Display = i.InvoiceNumber + " - " + (i.Customer != null ? i.Customer.Name : "") })
				    .ToListAsync();
				return View(receipt);
			}

			_context.Receipts.Add(receipt);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Details(int id)
		{
			var payment = await _context.Receipts
			    .Join(_context.Customers,
					p => p.CustomerId,
					s => s.Id,
					(p, s) => new ReceiptDetailDto
					{
						Id = p.Id,
						CustomerId = p.CustomerId,
						CustomerName = s.Name,
						PaymentMode = p.PaymentMode,
						Amount = p.Amount,
						Description = p.Description ?? "",
						WithGst = p.WithGst,
						Date = p.Date
					})
			    .FirstOrDefaultAsync(m => m.Id == id);
			if (payment == null)
			{
				return NotFound();
			}

			return View(payment);
		}


		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var payment = await _context.Receipts.FindAsync(id);
			if (payment == null) return NotFound();

			ViewBag.Customers = await _context.Customers.ToListAsync();
			ViewBag.PaymentModes = Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>().ToList();
			return View(payment);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, EditReceiptDto request)
		{
			if (id != request.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var payment = await _context.Receipts.FindAsync(id);
					if (payment == null)
					{
						return NotFound();
					}
					payment.CustomerId = request.CustomerId;
					payment.PaymentMode = request.PaymentMode;
					payment.Amount = request.Amount;
					payment.Description = request.Description;
					payment.Date = request.Date;
					_context.Update(payment);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ReceiptExists(request.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewBag.Customers = await _context.Customers.ToListAsync();
			ViewBag.PaymentModes = System.Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>().ToList();
			return View(request);
		}

		[Authorize(Roles = "Admin")]
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var payment = await _context.Receipts
			    .Join(_context.Customers,
					p => p.CustomerId,
					s => s.Id,
					(p, s) => new ReceiptDetailDto
					{
						Id = p.Id,
						CustomerId = p.CustomerId,
						CustomerName = s.Name,
						PaymentMode = p.PaymentMode,
						Amount = p.Amount,
						Description = p.Description,
						Date = p.Date
					})
			    .FirstOrDefaultAsync(m => m.Id == id);
			if (payment == null)
			{
				return NotFound();
			}

			return View(payment);
		}

		[HttpPost, ActionName("Delete")]
		[Authorize(Roles = "Admin")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var payment = await _context.Receipts.FindAsync(id);
			_context.Receipts.Remove(payment);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
		private bool ReceiptExists(int id)
		{
			return _context.Receipts.Any(e => e.Id == id);
		}
	}
}
