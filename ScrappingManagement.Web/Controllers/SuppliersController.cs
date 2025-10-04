using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers
{
	public class SuppliersController : Controller
	{
		private readonly AppDbContext _context;

		public SuppliersController(AppDbContext context)
		{
			_context = context;
		}

		// GET: suppliers
		public async Task<IActionResult> Index()
		{
			var suppliersWithDueAmount = await _context.Suppliers.AsNoTracking()
			    .Select(s => new SupplierListDto
			    {
				    Id = s.Id,
				    Name = s.Name,
				    Location = s.Location,
				    DueAmount = _context.Quotes
								   .Where(q => q.SupplierId == s.Id).AsNoTracking()
								   .Sum(q => (decimal?)q.FinalTotal ?? 0) // Total Quotes
							- _context.Payments
								   .Where(p => p.SupplierId == s.Id).AsNoTracking()
								   .Sum(p => (decimal?)p.Amount ?? 0) // Total Payments
			    })
			    .ToListAsync();

			return View(suppliersWithDueAmount);
		}

		// GET: suppliers/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();
			var supplier = await _context.Suppliers.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		// GET: suppliers/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: suppliers/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Name,Location")] Supplier supplier)
		{
			if (ModelState.IsValid)
			{
				_context.Add(supplier);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(supplier);
		}

		// GET: suppliers/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();
			var supplier = await _context.Suppliers.FindAsync(id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		// POST: suppliers/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Location")] Supplier supplier)
		{
			if (id != supplier.Id) return NotFound();
			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(supplier);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Suppliers.Any(e => e.Id == supplier.Id))
						return NotFound();
					else
						throw;
				}
				return RedirectToAction(nameof(Index));
			}
			return View(supplier);
		}

		// GET: suppliers/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();
			var supplier = await _context.Suppliers.FirstOrDefaultAsync(m => m.Id == id);
			if (supplier == null) return NotFound();
			return View(supplier);
		}

		// POST: suppliers/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var supplier = await _context.Suppliers.FindAsync(id);
			if (supplier != null)
			{
				_context.Suppliers.Remove(supplier);
				await _context.SaveChangesAsync();
			}
			return RedirectToAction(nameof(Index));
		}
	}
}