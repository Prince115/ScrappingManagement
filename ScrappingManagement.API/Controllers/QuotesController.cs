using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.API.Data;
using ScrappingManagement.API.DTO;
using ScrappingManagement.API.Models;

namespace ScrappingManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class QuotesController : ControllerBase
{
	private readonly AppDbContext _context;

	public QuotesController(AppDbContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Quote>>> GetTransactions()
	{
		return await _context.Quotes
		    .Include(t => t.QuoteProducts)
		    .ToListAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<QuoteDetailDto>> GetTransaction(int id)
	{
		var a = _context.Products;
		var quote = await _context.Quotes
					.Include(q=>q.QuoteProducts)
				    .Where(q => q.Id == id)
				    .Select(q => new QuoteDetailDto
				    {
					    Id = q.Id,
					    Date = q.Date,
					    Location = q.Location,
					    SupplierId = q.SupplierId,
					    FinalTotal = q.QuoteProducts.Sum(e => e.TotalAmount),
					    QuoteProducts = q.QuoteProducts
						  .Join(_context.Products,
							   qp => qp.ProductId,
							   p => p.Id,
							   (qp, p) => new QuoteProductDetailDto
							   {
								   Id = qp.Id,
								   LoadedWeight = qp.LoadedWeight,
								   UnloadWeight = qp.UnloadWeight,
								   Gross = qp.Gross,
								   NetWeight = qp.NetWeight,
								   Rate = qp.Rate,
								   TotalAmount = qp.TotalAmount,
								   ProductId = p.Id,
								   Name = p.Name
							   })
						  .ToList()
				    })
				    .FirstOrDefaultAsync();


		if (quote == null)
			return NotFound();

		return quote;
	}

	[HttpPost]
	public async Task<ActionResult<Quote>> CreateTransaction(Quote transaction)
	{
		_context.Quotes.Add(transaction);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteTransaction(int id)
	{
		var transaction = await _context.Quotes
		    .Include(t => t.QuoteProducts)
		    .FirstOrDefaultAsync(t => t.Id == id);

		if (transaction == null)
			return NotFound();

		_context.Quotes.Remove(transaction);
		await _context.SaveChangesAsync();

		return NoContent();
	}
}
