using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;

namespace ScrappingManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,User")]
    public class CustomerLedgerController : Controller
    {
        private readonly AppDbContext _context;

        public CustomerLedgerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? selectedCustomerId, 
            DateTime? fromDate, 
            DateTime? toDate)
        {
            ViewBag.Customers = await _context.Customers.OrderBy(s => s.Name).ToListAsync();

            var model = new CustomerLedgerDtos
            {
                SelectedCustomerId = selectedCustomerId,
                FromDate = fromDate,
                ToDate = toDate
            };

            if (selectedCustomerId.HasValue)
            {
                model.SelectedCustomerName = (await _context.Customers.FindAsync(selectedCustomerId.Value))?.Name;

                var Invoices = _context.Invoices
                    .Where(q => q.CustomerId == selectedCustomerId.Value)
                    .Select(q => new LedgerEntryDto
                    {
                        Date = q.Date,
                        Type = "Invoice",
                        Description =  q.Id.ToString(),
                        Debit = q.FinalAmount, 
                        Credit = 0,
                        DocumentId = q.Id
                    });

                var Receipts = _context.Receipts
                    .Where(p => p.CustomerId == selectedCustomerId.Value)
                    .Select(p => new LedgerEntryDto
                    {
                        Date = p.Date,
                        Type = "Receipt",
                        Description =  p.PaymentMode.ToString(),
                        Debit = 0,
                        Credit = p.Amount,  
                        DocumentId = p.Id
                    });

                if (fromDate.HasValue)
                {
                    Invoices = Invoices.Where(q => q.Date >= fromDate.Value);
                    Receipts = Receipts.Where(p => p.Date >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    Invoices = Invoices.Where(q => q.Date <= toDate.Value);
                    Receipts = Receipts.Where(p => p.Date <= toDate.Value);
                }

                model.LedgerEntries.AddRange(await Invoices.ToListAsync());
                model.LedgerEntries.AddRange(await Receipts.ToListAsync());

                model.LedgerEntries = [.. model.LedgerEntries.OrderBy(e => e.Date)];

                model.TotalDebit = model.LedgerEntries.Where(e => e.Type == "Invoice").Sum(e => e.Debit);
                model.TotalCredit = model.LedgerEntries.Where(e => e.Type == "Receipt").Sum(e => e.Credit);
                model.DueAmount = model.TotalDebit - model.TotalCredit;
            }

            return View(model);
        }
    }
}
