using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.API.Data;
using ScrappingManagement.API.Models;

namespace ScrappingManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SuppliersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Suppliers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers()
        {
            return await _context.Suppliers
                .ToListAsync();
        }

        // GET: api/Suppliers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier(int id)
        {
            var Supplier = await _context.Suppliers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Supplier == null)
                return NotFound();

            return Supplier;
        }

        // POST: api/Suppliers
        [HttpPost]
        public async Task<ActionResult<Supplier>> AddSupplier(Supplier Supplier)
        {
            _context.Suppliers.Add(Supplier);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSupplier), new { id = Supplier.Id }, Supplier);
        }

        // PUT: api/Suppliers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSupplier(int id, Supplier updatedSupplier)
        {
            if (id != updatedSupplier.Id)
                return BadRequest();

            var existing = await _context.Suppliers.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = updatedSupplier.Name;
            existing.Location = updatedSupplier.Location;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Suppliers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var Supplier = await _context.Suppliers
                .FirstOrDefaultAsync(c => c.Id == id);

            if (Supplier == null)
                return NotFound();

            _context.Suppliers.Remove(Supplier);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
