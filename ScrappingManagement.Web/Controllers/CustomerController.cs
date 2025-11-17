using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers;

public class CustomersController(AppDbContext context) : Controller
{
	private readonly AppDbContext _context = context;

    public async Task<IActionResult> Index()
	{
		var customers = await _context.Customers.AsNoTracking()
			    .Select(s => new CustomerListDto
			    {
				    Id = s.Id,
				    Name = s.Name,
				    Location = s.Location,
				    DueAmount = (s.OpeningBalance ?? 0) 
							+ _context.Invoices
								   .Where(q => q.CustomerId == s.Id).AsNoTracking()
								   .Sum(q => (decimal?)q.FinalAmount ?? 0)
							- _context.Receipts
								   .Where(p => p.CustomerId == s.Id).AsNoTracking()
								   .Sum(p => (decimal?)p.Amount ?? 0)
			    })
			    .ToListAsync();
		return View(customers);
	}

	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(Customer customer)
	{
		if (ModelState.IsValid)
		{
			_context.Customers.Add(customer);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		return View(customer);
	}

	public async Task<IActionResult> Edit(int id)
	{
		var customer = await _context.Customers.FindAsync(id);
		if (customer == null) return NotFound();
		return View(customer);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, Customer customer)
	{
		if (id != customer.Id) return NotFound();

		if (ModelState.IsValid)
		{
			_context.Update(customer);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		return View(customer);
	}

	public async Task<IActionResult> Delete(int id)
	{
		var customer = await _context.Customers.FindAsync(id);
		if (customer == null) return NotFound();

		return View(customer);
	}

	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		var customer = await _context.Customers.FindAsync(id);
		_context.Customers.Remove(customer);
		await _context.SaveChangesAsync();
		return RedirectToAction(nameof(Index));
	}
}
