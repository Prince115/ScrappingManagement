using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.API.Data;
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
    public async Task<ActionResult<Quote>> GetTransaction(int id)
    {
        var transaction = await _context.Quotes
            .Include(t => t.QuoteProducts)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        return transaction;
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
