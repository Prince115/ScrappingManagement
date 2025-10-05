using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class QuotesController : Controller
    {
        private readonly ILogger<QuotesController> _logger;
        private readonly AppDbContext _context;
        public QuotesController(ILogger<QuotesController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;

        }

        public async Task<IActionResult> Index(int? pageNumber, int? pageSize, int? supplierId, DateTime? fromDate, DateTime? toDate, QuoteStatus? status)
        {
			
			int currentPageSize = pageSize ?? 20;
			var quotes = _context.Quotes
					   .Include(q => q.Supplier)
					   .Include(q => q.QuoteProducts)
					  .AsQueryable();

			// Apply filters
			if (supplierId.HasValue)
			{
				quotes = quotes.Where(q => q.SupplierId == supplierId.Value);
			}

			if (fromDate.HasValue)
			{
				quotes = quotes.Where(q => q.Date >= fromDate.Value.Date);
			}

			if (toDate.HasValue)
			{
				quotes = quotes.Where(q => q.Date <= toDate.Value.Date);
			}

			if (status.HasValue)
			{
				quotes = quotes.Where(q => q.Status == status.Value);
			}

			quotes = quotes.OrderByDescending(q => q.Id);

			ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();
			ViewBag.QuoteStatuses = new SelectList(Enum.GetValues(typeof(QuoteStatus)).Cast<QuoteStatus>());
			ViewData["CurrentSupplierFilter"] = supplierId;
			ViewData["CurrentFromDateFilter"] = fromDate?.ToString("yyyy-MM-dd");
			ViewData["CurrentToDateFilter"] = toDate?.ToString("yyyy-MM-dd");
			ViewData["CurrentPageSize"] = currentPageSize;
			ViewData["CurrentStatusFilter"] = status;

			return View(await PaginatedList<Quote>.CreateAsync(quotes.AsNoTracking(), pageNumber ?? 1, currentPageSize));

		}

		public async Task<IActionResult> CreateAsync()
		{

			ViewBag.Suppliers = await _context.Suppliers.OrderBy(o => o.Name).ToListAsync();
			ViewBag.Products = await _context.Products.OrderBy(o => o.Name).ToListAsync();
			ViewBag.QuoteStatuses = new SelectList(Enum.GetValues(typeof(QuoteStatus)).Cast<QuoteStatus>());

			var maxBillNumber = await _context.Quotes.CountAsync();

			int nextBillNumberInt = 1;
			if (maxBillNumber > 0)
			{
				nextBillNumberInt = maxBillNumber + 1;
			}
			ViewBag.NextBillNumber = nextBillNumberInt;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Date,Location,SupplierId,Total,FinalTotal,Kato,Status")] Quote quote, List<QuoteProduct> quoteProducts)
		{
			if (ModelState.IsValid)
			{
				quote.QuoteProducts = quoteProducts;
				_context.Quotes.Add(quote);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Suppliers = await _context.Suppliers.OrderBy(o => o.Name).ToListAsync();
			ViewBag.Products = await _context.Products.OrderBy(o => o.Name).ToListAsync();
			ViewBag.QuoteStatuses = new SelectList(Enum.GetValues(typeof(QuoteStatus)).Cast<QuoteStatus>(), quote.Status);

			return View(quote);
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();


			var quote = await _context.Quotes
			    .Where(q => q.Id == id)
			    .Select(q => new QuoteDetailDto
			    {
				    Id = q.Id,
				    Date = q.Date,
				    Location = q.Location,
				    SupplierName = q.Supplier.Name,
				    FinalTotal = q.FinalTotal,
				    BillNumber = q.BillNumber,
				    Kato = q.Kato,
				    Total = q.Total,
				    Status = q.Status, // Add Status
				    Products = q.QuoteProducts
					  .Join(_context.Products,
						   qp => qp.ProductId,
						   p => p.Id,
						   (qp, p) => new QuoteProductDetailDto
						   {
							   Id = qp.Id,
							   Nos = qp.Nos,
							   ProductName = p.Name,
							   LoadedWeight = qp.LoadedWeight ?? 0,
							   UnloadWeight = qp.UnloadWeight ?? 0,
							   Gross = qp.Gross,
							   BoraCount = qp.BoraCount,
							   BoraReport = qp.BoraReport,
							   ProductReport = qp.ProductReport,
							   NetWeight = qp.NetWeight,
							   Rate = qp.Rate,
							   TotalAmount = qp.TotalAmount
						   })
					  .ToList()
			    })
			    .FirstOrDefaultAsync();

			if (quote == null) return NotFound();

			return View(quote);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var quote = await _context.Quotes
			    .Include(q => q.QuoteProducts)
			    .FirstOrDefaultAsync(m => m.Id == id);

			if (quote == null)
			{
				return NotFound();
			}

			ViewBag.Suppliers = await _context.Suppliers.OrderBy(o => o.Name).ToListAsync();
			ViewBag.Products = await _context.Products.OrderBy(o => o.Name).ToListAsync();
			ViewBag.QuoteStatuses = new SelectList(Enum.GetValues(typeof(QuoteStatus)).Cast<QuoteStatus>(), quote.Status);

			return View(quote);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Location,SupplierId,Total,FinalTotal,Kato,Status")] Quote quote, List<QuoteProduct> quoteProducts)
		{
			if (id != quote.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var existingQuote = await _context.Quotes
					    .Include(q => q.QuoteProducts)
					    .FirstOrDefaultAsync(q => q.Id == id);

					if (existingQuote == null)
					{
						return NotFound();
					}

					// Update main quote properties
					existingQuote.Date = quote.Date;
					existingQuote.Location = quote.Location;
					existingQuote.SupplierId = quote.SupplierId;
					existingQuote.FinalTotal = quote.FinalTotal;
					existingQuote.Kato = quote.Kato;
					existingQuote.Total = quote.Total;
					existingQuote.Status = quote.Status; // Update Status

					// Update QuoteProducts
					// Remove products not in the submitted list
					//existingQuote.QuoteProducts
					//    .RemoveAll(p => !quoteProducts.Any(qp => qp.Id == p.Id && qp.Id != 0));
					existingQuote.QuoteProducts = existingQuote.QuoteProducts
										    .Where(p => quoteProducts.Any(qp => qp.Id == p.Id && qp.Id != 0))
										    .ToList();

					foreach (var product in quoteProducts)
					{
						if (product.Id == 0)
						{
							existingQuote.QuoteProducts.Add(product);
						}
						else
						{
							var existingProduct = existingQuote.QuoteProducts.FirstOrDefault(p => p.Id == product.Id);
							if (existingProduct != null)
							{
								existingProduct.ProductId = product.ProductId;
								existingProduct.LoadedWeight = product.LoadedWeight;
								existingProduct.UnloadWeight = product.UnloadWeight;
								existingProduct.BoraCount = product.BoraCount;
								existingProduct.BoraReport = product.BoraReport;
								existingProduct.ProductReport = product.ProductReport;
								existingProduct.Rate = product.Rate;
								existingProduct.Gross = product.Gross;
								existingProduct.TotalAmount = product.TotalAmount;
								existingProduct.Nos = product.Nos;
							}
						}
					}

					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!QuoteExists(quote.Id))
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
			ViewBag.Suppliers = await _context.Suppliers.OrderBy(o => o.Name).ToListAsync();
			ViewBag.Products = await _context.Products.OrderBy(o => o.Name).ToListAsync();

			return View(quote);
		}

		private bool QuoteExists(int id)
		{
			return _context.Quotes.Any(e => e.Id == id);
		}

		public async Task<IActionResult> Print(int? id)
		{
			if (id == null) return NotFound();

			var quote = await _context.Quotes
				.Where(q => q.Id == id)
				.Select(q => new QuoteDetailDto
				{
					Id = q.Id,
					Date = q.Date,
					Location = q.Location,
					SupplierName = q.Supplier.Name,
					FinalTotal = q.FinalTotal,
					Total = q.Total,
					BillNumber = q.BillNumber,
					Kato = q.Kato,
					Status = q.Status, // Add Status
					Products = q.QuoteProducts
					  .Join(_context.Products,
						   qp => qp.ProductId,
						   p => p.Id,
						   (qp, p) => new QuoteProductDetailDto
						   {
							   Id = qp.Id,
							   ProductName = p.Name,
							   LoadedWeight = qp.LoadedWeight ?? 0,
							   UnloadWeight = qp.UnloadWeight ?? 0,
							   Gross = qp.Gross,
							   BoraCount = qp.BoraCount,
							   BoraReport = qp.BoraReport,
							   ProductReport = qp.ProductReport,
							   NetWeight = qp.NetWeight,
							   Rate = qp.Rate,
							   TotalAmount = qp.TotalAmount,
							   Nos = qp.Nos
						   })
					  .ToList()
				})
				.FirstOrDefaultAsync();

			if (quote == null) return NotFound();

			return View(quote);
		}

		[Authorize(Roles = "Admin")] 
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var quote = await _context.Quotes
				.Include(q => q.Supplier)
				.FirstOrDefaultAsync(m => m.Id == id);

			if (quote == null) return NotFound();

			return View(quote);
		}

		// POST: Quotes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		[Authorize(Roles = "Admin")] // Only Admin can delete
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var quote = await _context.Quotes
				.Include(q => q.QuoteProducts)
				.FirstOrDefaultAsync(q => q.Id == id);

			if (quote == null)
			{
				return NotFound();
			}

			_context.QuoteProducts.RemoveRange(quote.QuoteProducts);
			_context.Quotes.Remove(quote);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}
