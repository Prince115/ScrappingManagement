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
	[Authorize(Roles = "Admin,User,QuotesUser")]
	public class QuotesController : Controller
	{
		private readonly ILogger<QuotesController> _logger;
		private readonly AppDbContext _context;
		public QuotesController(ILogger<QuotesController> logger, AppDbContext context)
		{
			_logger = logger;
			_context = context;
		}

		public async Task<IActionResult> Index(int? pageNumber, int? pageSize, int? supplierId, DateOnly? fromDate, DateOnly? toDate, QuoteStatus? status)
		{

			int currentPageSize = pageSize ?? 20;
			var quotes = _context.Quotes
					   .Include(q => q.Supplier)
					   .Include(q => q.QuoteProducts)
					  .AsQueryable();

			if (supplierId.HasValue)
			{
				quotes = quotes.Where(q => q.SupplierId == supplierId.Value);
			}

			if (fromDate.HasValue)
			{
				quotes = quotes.Where(q => q.Date >= fromDate.Value);
			}

			if (toDate.HasValue)
			{
				quotes = quotes.Where(q => q.Date <= toDate.Value);
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

			var maxBillNumber = await _context.Quotes.OrderByDescending(a => Convert.ToInt16(a.BillNumber ?? "0")).FirstOrDefaultAsync();
			int nextBillNumberInt = 1;
			if (maxBillNumber is not null || Convert.ToInt16(maxBillNumber?.BillNumber) > 0)
			{
				nextBillNumberInt = Convert.ToInt16(maxBillNumber?.BillNumber) + 1;
			}
			ViewBag.NextBillNumber = nextBillNumberInt;
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Date,Location,SupplierId,Total,FinalTotal,Kato,Note,Status,BillNumber,PaymentAmount")] Quote quote, List<QuoteProduct> quoteProducts)
		{
			if (ModelState.IsValid)
			{
				var maxBillNumber = await _context.Quotes.OrderByDescending(a => Convert.ToInt16(a.BillNumber ?? "0")).FirstOrDefaultAsync();
				int nextBillNumberInt = 1;
				if (maxBillNumber is not null || Convert.ToInt16(maxBillNumber?.BillNumber) > 0)
				{
					nextBillNumberInt = Convert.ToInt16(maxBillNumber?.BillNumber) + 1;
				}
				quote.BillNumber = nextBillNumberInt.ToString();
				quote.QuoteProducts = quoteProducts;
                _context.Quotes.Add(quote);

                // If the quote is not completed and there is an initial payment amount, create a payment record for it
                if (quote.Status != QuoteStatus.Completed && quote.PaymentAmount > 0)
                {
                    _context.Payments.Add(new Payment
                    {
                        Amount = (decimal)quote.PaymentAmount,
                        Date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
                        SupplierId = quote.SupplierId,
                        PaymentMode = PaymentMode.Cash,
                        Description = "Initial Payment",
                        Quote = quote
                    });
                }

                if (quote.Status == QuoteStatus.Completed)
				{
					var vRemainingAmount = quote.FinalTotal - (quote.PaymentAmount ?? 0);

                    _context.Payments.Add(new Payment
					{
						Amount = vRemainingAmount,
						Date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
						SupplierId = quote.SupplierId,
						PaymentMode = PaymentMode.Cash,
						Description = "Auto Created",
                        Quote = quote
                    });
				}
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			ViewBag.Suppliers = await _context.Suppliers.OrderBy(o => o.Name).ToListAsync();
			ViewBag.Products = await _context.Products.OrderBy(o => o.Name).ToListAsync();
			ViewBag.QuoteStatuses = new SelectList(Enum.GetValues(typeof(QuoteStatus)).Cast<QuoteStatus>(), quote.Status);

			return View(quote);
		}

		[HttpPost]
		[Route("Quote/UpdateStatus")]
		public IActionResult UpdateStatus([FromBody] UpdateQuoteStatusDto dto)
		{
			if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
				return BadRequest("Invalid data.");

			var quote = _context.Quotes.FirstOrDefault(q => q.Id == dto.Id);
			if (quote == null)
				return NotFound("Quote not found.");

			try
			{
				if (Enum.TryParse<QuoteStatus>(dto.Status, out var parsedStatus))
				{
					quote.Status = parsedStatus;

                    // If the status is updated to Completed, create a payment record for the remaining amount
                    if (parsedStatus == QuoteStatus.Completed)
					{
                        var vRemainingAmount = quote.FinalTotal - (quote.PaymentAmount ?? 0);
                        _context.Payments.Add(new Payment
						{
							Amount = vRemainingAmount,
							Date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
							SupplierId = quote.SupplierId,
							PaymentMode = PaymentMode.Cash,
							Description = "Auto Created",
							QuoteID = quote.Id,
						});
					}
					_context.SaveChanges();
					return Ok(new { success = true });
				}
				else
				{
					return BadRequest("Invalid status value.");
				}
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { success = false, message = ex.Message });
			}
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
				    Note = q.Note,
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
			    .Include(q => q.QuoteProducts.OrderBy(o => o.Id))
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
		public async Task<IActionResult> Edit(int id, [Bind("Id,Date,Location,SupplierId,Total,FinalTotal,Note,Kato,Status,PaymentAmount")] Quote quote, List<QuoteProduct> quoteProducts)
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

					existingQuote.Date = quote.Date;
					existingQuote.Location = quote.Location;
					existingQuote.SupplierId = quote.SupplierId;
					existingQuote.FinalTotal = quote.FinalTotal;
					existingQuote.Kato = quote.Kato;
					existingQuote.Total = quote.Total;
					existingQuote.Note = quote.Note;
					existingQuote.PaymentAmount = quote.PaymentAmount;

                    // If the quote is not completed and there is an initial payment amount, create or update a payment record for it
                    if (quote.Status != QuoteStatus.Completed && quote.PaymentAmount > 0)
                    {
                        var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.QuoteID == id);

                        if (existingPayment == null)
                        {
                            await _context.Payments.AddAsync(new Payment
                            {
                                Amount = (decimal)quote.PaymentAmount,
                                Date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
                                SupplierId = quote.SupplierId,
                                PaymentMode = PaymentMode.Cash,
                                Description = "Initial Payment",
                                QuoteID = id
                            });
                        }
                        else
                        {
                            existingPayment.Amount = (decimal)quote.PaymentAmount;
                        }
                    }

                    foreach (var product in quoteProducts)
					{
						if (product.Id == 0 && product.Deleted == 0)
						{
							existingQuote.QuoteProducts.Add(product);
						}
						else
						{
							if (product.Id == 0 && product.Deleted == 1)
							{
								continue;
							}
							var existingProduct = existingQuote.QuoteProducts.FirstOrDefault(p => p.Id == product.Id);
							if (existingProduct != null)
							{
								if (product.Deleted == 1)
								{
									existingQuote.QuoteProducts.Remove(existingProduct);
								}
								else
								{
									existingProduct.ProductId = product.ProductId;
									if ((product.Nos ?? 0) <= 0)
									{
										existingProduct.LoadedWeight = product.LoadedWeight;
										existingProduct.UnloadWeight = product.UnloadWeight;
									}
									else
									{
										existingProduct.UnloadWeight = existingProduct.LoadedWeight = null;
									}
									existingProduct.BoraCount = product.BoraCount;
									existingProduct.BoraReport = product.BoraReport;
									existingProduct.ProductReport = product.ProductReport;
									existingProduct.Rate = product.Rate;
									existingProduct.Description = product.Description;
									existingProduct.Gross = product.Gross;
									existingProduct.TotalAmount = product.TotalAmount;
									existingProduct.Nos = product.Nos;
									existingProduct.NetWeight = product.NetWeight;
								}
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
					Status = q.Status,
					Note = q.Note,
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
