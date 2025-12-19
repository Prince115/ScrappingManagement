using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;
using ScrappingManagement.Web.Helpers;
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

	public async Task<IActionResult> Index(int? pageNumber, int? pageSize, int? customerId, DateOnly? fromDate, DateOnly? toDate)
	{

		int currentPageSize = pageSize ?? 20;
		var quotes = _context.Invoices
				   .Include(q => q.Customer)
				   .Include(q => q.Items)
				  .AsQueryable();

		if (customerId.HasValue)
		{
			quotes = quotes.Where(q => q.CustomerId == customerId.Value);
		}

		if (fromDate.HasValue)
		{
			quotes = quotes.Where(q => q.Date >= fromDate.Value);
		}

		if (toDate.HasValue)
		{
			quotes = quotes.Where(q => q.Date <= toDate.Value);
		}

		quotes = quotes.OrderByDescending(q => q.Id);

		ViewBag.Customers = await _context.Customers.OrderBy(s => s.Name).ToListAsync();
		ViewData["CurrentCustomerFilter"] = customerId;
		ViewData["CurrentFromDateFilter"] = fromDate?.ToString("yyyy-MM-dd");
		ViewData["CurrentToDateFilter"] = toDate?.ToString("yyyy-MM-dd");
		ViewData["CurrentPageSize"] = currentPageSize;

		return View(await PaginatedList<Invoice>.CreateAsync(quotes.AsNoTracking(), pageNumber ?? 1, currentPageSize));

	}

	public async Task<IActionResult> Create()
	{
		var viewModel = new InvoiceViewModel
		{
			Date = DateOnly.FromDateTime(DateTime.UtcNow.ToIndianTime()),
			Customers = [.. _context.Customers.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })],
			Products = [.. _context.Products.Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })],
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
				_ = int.TryParse(parts, out lastNumber);
			}
			else
			{
				var parts = lastInvoice.InvoiceNumber;
				_ = int.TryParse(parts, out lastNumber);
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

		// compute subtotal from items (prefer using Weight*Rate to avoid trusting client Amount)
		var items = (vm.Items ?? [])
			.Where(i => !i.Deleted && i.ProductId != 0)
			.Select(i => new InvoiceItem
			{
				ProductId = i.ProductId,
				Rate = i.Rate,
				Weight = i.Weight,
				Amount = i.Weight * i.Rate
			})
			.ToList();

		var subtotal = items.Sum(i => i.Amount);
		var packaging = vm.PackagingCharge;
		var baseAmount = subtotal + packaging;

		var gstValue = 0m;
		if (vm.WithGst && vm.GstPercentage > 0)
		{
			gstValue = baseAmount * (vm.GstPercentage / 100m);
			baseAmount += gstValue;
		}

		var tcsValue = 0m;
		if (vm.TcsPercentage > 0)
		{
			tcsValue = baseAmount * (vm.TcsPercentage / 100m);
			baseAmount += tcsValue;
		}

		var invoice = new Invoice
		{
			InvoiceNumber = vm.InvoiceNumber,
			Date = vm.Date,
			CustomerId = vm.CustomerId,
			Location = vm.Location,
			Note = vm.Note,
			PackagingCharge = vm.PackagingCharge,
			GstPercentage = vm.GstPercentage,
			GstValue = gstValue,
			WithGst = vm.WithGst,
			TcsPercentage = vm.TcsPercentage,
			BillNo = vm.BillNo,
			BookNo = vm.BookNo,
			TcsValue = tcsValue,
			FinalAmount = baseAmount,
			Items = items
		};

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
			GstPercentage = invoice.GstPercentage,
			GstValue = invoice.GstValue,
			WithGst = invoice.WithGst,
			Note = invoice.Note,
			BillNo = invoice.BillNo,
			BookNo = invoice.BookNo,
			FinalAmount = invoice.FinalAmount,
			PackagingCharge = invoice.PackagingCharge,
			Items = [.. invoice.Items!.Select(i => new InvoiceItemViewModel
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

		invoice.Date = vm.Date;
		invoice.CustomerId = vm.CustomerId;
		invoice.Location = vm.Location;
		invoice.Note = vm.Note;
		invoice.PackagingCharge = vm.PackagingCharge;
		invoice.WithGst = vm.WithGst;
		invoice.GstPercentage = vm.GstPercentage;
		invoice.BillNo = vm.BillNo;
		invoice.BookNo = vm.BookNo;

		var incoming = vm.Items ?? [];
		var deletedIds = incoming.Where(x => x.Deleted && x.Id != 0).Select(x => x.Id).ToHashSet();
		if (deletedIds.Count != 0)
		{
			var toDelete = invoice.Items!.Where(ii => deletedIds.Contains(ii.Id)).ToList();
			if (toDelete.Count != 0)
			{
				_context.InvoiceItems.RemoveRange(toDelete);
				invoice.Items = [.. invoice.Items!.Where(ii => !deletedIds.Contains(ii.Id))];
			}
		}

		var incomingIds = incoming.Where(x => x.Id != 0 && !x.Deleted).Select(x => x.Id).ToHashSet();
		var implicitlyRemoved = invoice.Items!.Where(ii => !incomingIds.Contains(ii.Id)).ToList();
		if (implicitlyRemoved.Count != 0)
		{
			_context.InvoiceItems.RemoveRange(implicitlyRemoved);
			invoice.Items = [.. invoice.Items!.Where(ii => incomingIds.Contains(ii.Id))];
		}

		foreach (var itemVm in incoming.Where(i => !i.Deleted))
		{
			if (itemVm.Id == 0)
			{
				invoice.Items!.Add(new InvoiceItem
				{
					ProductId = itemVm.ProductId,
					Weight = itemVm.Weight,
					Rate = itemVm.Rate,
					Amount = itemVm.Weight * itemVm.Rate
				});
			}
			else
			{
				var existing = invoice.Items!.FirstOrDefault(ii => ii.Id == itemVm.Id);
				if (existing != null)
				{
					existing.ProductId = itemVm.ProductId;
					existing.Weight = itemVm.Weight;
					existing.Rate = itemVm.Rate;
					existing.Amount = itemVm.Weight * itemVm.Rate;
				}
			}
		}

		var subtotal = invoice.Items!.Sum(i => i.Amount);
		var baseAmount = subtotal + invoice.PackagingCharge;

		var gstValue = 0m;
		if (invoice.WithGst && invoice.GstPercentage > 0)
		{
			gstValue = baseAmount * (invoice.GstPercentage / 100m);
			baseAmount += gstValue;
		}
		invoice.GstValue = gstValue;

		var tcsValue = 0m;
		invoice.TcsPercentage = vm.TcsPercentage;
		if (invoice.TcsPercentage > 0)
		{
			tcsValue = baseAmount * (invoice.TcsPercentage / 100m);
			baseAmount += tcsValue;
		}
		invoice.TcsValue = tcsValue;

		invoice.FinalAmount = baseAmount;

		await _context.SaveChangesAsync();
		return RedirectToAction("Index");
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
