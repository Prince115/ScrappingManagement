using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ScrappingManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? pageNumber,
            int? pageSize,
            int? supplierId,
            PaymentMode? paymentMode,
            DateTime? fromDate,
            DateTime? toDate)
        {
			int currentPageSize = pageSize ?? 20;

			var payments = _context.Payments.AsQueryable();

			// Apply filters
			if (supplierId.HasValue)
			{
				payments = payments.Where(p => p.SupplierId == supplierId.Value);
			}

			if (paymentMode.HasValue)
			{
				payments = payments.Where(p => p.PaymentMode == paymentMode.Value);
			}

			if (fromDate.HasValue)
			{
				payments = payments.Where(p => p.Date >= fromDate.Value.Date);
			}

			if (toDate.HasValue)
			{
				payments = payments.Where(p => p.Date <= toDate.Value.Date);
			}

			// Join with Suppliers to get SupplierName and project into PaymentDetailDto
			var paymentDetails = payments
			    .Join(
				   _context.Suppliers,
				   payment => payment.SupplierId,
				   supplier => supplier.Id,
				   (payment, supplier) => new PaymentDetailDto
				   {
					   Id = payment.Id,
					   SupplierId = payment.SupplierId,
					   SupplierName = supplier.Name,
					   PaymentMode = payment.PaymentMode,
					   Amount = payment.Amount,
					   Description = payment.Description,
					   Date = payment.Date
				   }
			    )
			    .OrderByDescending(p => p.Id);

			ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
			ViewBag.PaymentModes = new SelectList(Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>());
			ViewData["CurrentSupplierFilter"] = supplierId;
			ViewData["CurrentPaymentModeFilter"] = paymentMode;
			ViewData["CurrentFromDateFilter"] = fromDate?.ToString("yyyy-MM-dd");
			ViewData["CurrentToDateFilter"] = toDate?.ToString("yyyy-MM-dd");
			ViewData["CurrentPageSize"] = currentPageSize;

			return View(await PaginatedList<PaymentDetailDto>.CreateAsync(paymentDetails.AsNoTracking(), pageNumber ?? 1, currentPageSize));
		}

		// GET: Payments/Create
		public async Task<IActionResult> Create()
		{
			ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
			ViewBag.PaymentModes = System.Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>().ToList();
			return View();
		}

		// POST: Payments/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreatePaymentDto createPaymentDto)
		{
			if (ModelState.IsValid)
			{
				var payment = new Payment
				{
					SupplierId = createPaymentDto.SupplierId,
					PaymentMode = createPaymentDto.PaymentMode,
					Amount = createPaymentDto.Amount,
					Description = createPaymentDto.Description,
					Date = createPaymentDto.Date
				};
				_context.Add(payment);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
			ViewBag.PaymentModes = System.Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>().ToList();
			return View(createPaymentDto);
		}

		// GET: Payments/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();

			var payment = await _context.Payments.FindAsync(id);
			if (payment == null) return NotFound();

			ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
			ViewBag.PaymentModes = System.Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>().ToList();
			return View(payment);
		}

		// POST: Payments/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, EditPaymentDto editPaymentDto)
		{
			if (id != editPaymentDto.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var payment = await _context.Payments.FindAsync(id);
					if (payment == null)
					{
						return NotFound();
					}
					payment.SupplierId = editPaymentDto.SupplierId;
					payment.PaymentMode = editPaymentDto.PaymentMode;
					payment.Amount = editPaymentDto.Amount;
					payment.Description = editPaymentDto.Description;
					payment.Date = editPaymentDto.Date;

					_context.Update(payment);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!PaymentExists(editPaymentDto.Id))
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
			ViewBag.Suppliers = await _context.Suppliers.ToListAsync();
			ViewBag.PaymentModes = System.Enum.GetValues(typeof(PaymentMode)).Cast<PaymentMode>().ToList();
			return View(editPaymentDto);
		}

		// GET: Payments/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var payment = await _context.Payments
			    .Join(_context.Suppliers,
					p => p.SupplierId,
					s => s.Id,
					(p, s) => new PaymentDetailDto
					{
						Id = p.Id,
						SupplierId = p.SupplierId,
						SupplierName = s.Name,
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

		// GET: Payments/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var payment = await _context.Payments
			    .Join(_context.Suppliers,
					p => p.SupplierId,
					s => s.Id,
					(p, s) => new PaymentDetailDto
					{
						Id = p.Id,
						SupplierId = p.SupplierId,
						SupplierName = s.Name,
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

		// POST: Payments/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var payment = await _context.Payments.FindAsync(id);
			_context.Payments.Remove(payment);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool PaymentExists(int id)
		{
			return _context.Payments.Any(e => e.Id == id);
		}
	}
}
