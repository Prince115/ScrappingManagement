using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScrappingManagement.Web.Data;
using ScrappingManagement.Web.Dto;

namespace ScrappingManagement.Web.Controllers
{
    public class SupplierLedgerController : Controller
    {
        private readonly AppDbContext _context;

        public SupplierLedgerController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            int? selectedSupplierId, 
            DateTime? fromDate, 
            DateTime? toDate)
        {
            ViewBag.Suppliers = await _context.Suppliers.OrderBy(s => s.Name).ToListAsync();

            var model = new SupplierLedgerDto
            {
                SelectedSupplierId = selectedSupplierId,
                FromDate = fromDate,
                ToDate = toDate
            };

            if (selectedSupplierId.HasValue)
            {
                model.SelectedSupplierName = (await _context.Suppliers.FindAsync(selectedSupplierId.Value))?.Name;

                var quotes = _context.Quotes
                    .Where(q => q.SupplierId == selectedSupplierId.Value)
                    .Select(q => new LedgerEntryDto
                    {
                        Date = q.Date,
                        Type = "Quote",
                        Description =  q.BillNumber,
                        Debit = q.FinalTotal, // Assuming FinalTotal is the debit amount for a quote
                        Credit = 0,
                        DocumentId = q.Id
                    });

                var payments = _context.Payments
                    .Where(p => p.SupplierId == selectedSupplierId.Value)
                    .Select(p => new LedgerEntryDto
                    {
                        Date = p.Date,
                        Type = "Payment",
                        Description =  p.PaymentMode.ToString(),
                        Debit = 0,
                        Credit = p.Amount,  
                        DocumentId = p.Id
                    });

                // Apply date range filters
                if (fromDate.HasValue)
                {
                    quotes = quotes.Where(q => q.Date >= fromDate.Value);
                    payments = payments.Where(p => p.Date >= fromDate.Value);
                }
                if (toDate.HasValue)
                {
                    quotes = quotes.Where(q => q.Date <= toDate.Value);
                    payments = payments.Where(p => p.Date <= toDate.Value);
                }

                model.LedgerEntries.AddRange(await quotes.ToListAsync());
                model.LedgerEntries.AddRange(await payments.ToListAsync());

                model.LedgerEntries = [.. model.LedgerEntries.OrderBy(e => e.Date)];

                model.TotalDebit = model.LedgerEntries.Where(e => e.Type == "Quote").Sum(e => e.Debit);
                model.TotalCredit = model.LedgerEntries.Where(e => e.Type == "Payment").Sum(e => e.Credit);
                model.DueAmount = model.TotalDebit - model.TotalCredit;
            }

            return View(model);
        }
    }
}
