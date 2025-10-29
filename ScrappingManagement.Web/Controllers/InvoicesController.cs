using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Controllers;
public class InvoicesController : Controller
{
	private readonly AppDbContext _context;
	private readonly IConfiguration _config;

	public InvoicesController(AppDbContext context, IConfiguration config)
	{
		_context = context;
		_config = config;
	}
	public async Task<IActionResult> Index()
	{
		var invoices = await _context.Invoices
		    .Include(i => i.Customer)
		    .OrderByDescending(i => i.Date)
		    .ToListAsync();

		return View(invoices);
	}
	public async Task<IActionResult> CreateAsync()
	{
		var viewModel = new InvoiceViewModel
		{
			Date = DateTime.Today,
			Customers = _context.Customers
			   .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
			Products = _context.Products
			   .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name }).ToList(),
			Items = [new InvoiceItemViewModel()]
		};
		var lastInvoice = await _context.Invoices
		.OrderByDescending(i => i.Id)
		.FirstOrDefaultAsync();

		int nextNumber = 1;

		if (lastInvoice != null && !string.IsNullOrEmpty(lastInvoice.InvoiceNumber))
		{
			var invoicePrefix = _config["InvoicePrefix"];
			int lastNumber = 0;
			if (!string.IsNullOrEmpty(invoicePrefix))
			{
				var parts = lastInvoice.InvoiceNumber.Replace(invoicePrefix, "");
				int.TryParse(parts, out lastNumber);
			}
			else
			{
				var parts = lastInvoice.InvoiceNumber;
				int.TryParse(parts, out lastNumber);
			}
			nextNumber = lastNumber + 1;
		}

		var newInvoiceNumber = $"{_config["InvoicePrefix"] ?? ""}{nextNumber}";
		viewModel.InvoiceNumber = newInvoiceNumber;
		return View(viewModel);
	}

	[HttpPost]
	public async Task<IActionResult> Create(InvoiceViewModel vm)
	{
		if (!ModelState.IsValid)
		{
			vm.Customers = await _context.Customers
				.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
				.ToListAsync();

			vm.Products = await _context.Products
				.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
				.ToListAsync();

			return View(vm);
		}
		var invoice = new Invoice
		{
			InvoiceNumber = vm.InvoiceNumber,
			Date = vm.Date,
			CustomerId = vm.CustomerId,
			Location = vm.Location,
			Note = vm.Note,
			PackagingCharge = vm.PackagingCharge,
			Items = [.. vm.Items.Select(i => new InvoiceItem
			{
				ProductId = i.ProductId,
				Rate = i.Rate,
				Weight = i.Weight,
				Amount = i.Amount
			})]
		};

		invoice.FinalAmount = invoice.Items.Sum(i => i.Rate * i.Weight) + invoice.PackagingCharge;

		_context.Invoices.Add(invoice);
		await _context.SaveChangesAsync();

		return RedirectToAction("Index", new { id = invoice.Id });
	}

	public async Task<IActionResult> Edit(int id)
	{
		var invoice = await _context.Invoices
		    .Include(i => i.Items)
		    .FirstOrDefaultAsync(i => i.Id == id);

		if (invoice == null)
			return NotFound();

		var viewModel = new InvoiceViewModel
		{
			InvoiceNumber = invoice.InvoiceNumber,
			Date = invoice.Date,
			CustomerId = invoice.CustomerId,
			Location = invoice.Location,
			Note = invoice.Note,
			PackagingCharge = invoice.PackagingCharge,
			Items = [.. invoice.Items.Select(i => new InvoiceItemViewModel
			{
				ProductId = i.ProductId,
				Weight = i.Weight,
				Amount = i.Amount,
				Rate = i.Rate
			})],
			Customers = await _context.Customers
			   .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
			   .ToListAsync(),
			Products = await _context.Products
			   .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
			   .ToListAsync()
		};

		return View(viewModel);
	}
	[HttpPost]
	public async Task<IActionResult> Edit(int id, InvoiceViewModel vm)
	{
		if (!ModelState.IsValid)
		{
			vm.Customers = await _context.Customers
			    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
			    .ToListAsync();
			vm.Products = await _context.Products
			    .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
			    .ToListAsync();
			return View(vm);
		}

		var invoice = await _context.Invoices
		    .Include(i => i.Items)
		    .FirstOrDefaultAsync(i => i.Id == id);

		if (invoice == null)
			return NotFound();

		// Update main invoice data
		invoice.InvoiceNumber = vm.InvoiceNumber;
		invoice.Date = vm.Date;
		invoice.CustomerId = vm.CustomerId;
		invoice.Location = vm.Location;
		invoice.Note = vm.Note;
		invoice.PackagingCharge = vm.PackagingCharge;

		// Remove existing items
		_context.InvoiceItems.RemoveRange(invoice.Items);

		// Add updated items
		invoice.Items = vm.Items.Select(i => new InvoiceItem
		{
			ProductId = i.ProductId,
			Weight = i.Weight,
			Amount = i.Amount,
			Rate = i.Rate
		}).ToList();

		// Recalculate final amount
		invoice.FinalAmount = invoice.Items.Sum(i => i.Rate * i.Weight) + vm.PackagingCharge;

		await _context.SaveChangesAsync();
		return RedirectToAction("Details", new { id = invoice.Id });
	}

	[HttpGet]
	public async Task<IActionResult> GetCustomer(int id)
	{
		var customer = await _context.Customers
		    .Where(c => c.Id == id)
		    .Select(c => new { c.Location })
		    .FirstOrDefaultAsync();

		if (customer == null) return NotFound();

		return Json(customer);
	}

	[HttpGet]
	public async Task<IActionResult> GetProduct(int id)
	{
		var product = await _context.Products
		    .Where(p => p.Id == id)
		    .Select(p => new { p.Rate })
		    .FirstOrDefaultAsync();

		if (product == null) return NotFound();

		return Json(product);
	}


}
