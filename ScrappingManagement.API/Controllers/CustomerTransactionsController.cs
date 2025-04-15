using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.API.Data;
using ScrappingManagement.API.Models;

namespace ScrappingManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CustomerTransactionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomerTransactionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerTransaction>>> GetTransactions()
    {
        return await _context.CustomerTransactions
            .Include(t => t.ProductEntries)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerTransaction>> GetTransaction(int id)
    {
        var transaction = await _context.CustomerTransactions
            .Include(t => t.ProductEntries)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        return transaction;
    }

    [HttpPost]
    public async Task<ActionResult<CustomerTransaction>> CreateTransaction(CustomerTransaction transaction)
    {
        _context.CustomerTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var transaction = await _context.CustomerTransactions
            .Include(t => t.ProductEntries)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return NotFound();

        _context.CustomerTransactions.Remove(transaction);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
