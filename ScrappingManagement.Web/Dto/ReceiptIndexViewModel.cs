using Microsoft.AspNetCore.Mvc.Rendering;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Dto
{
    public class ReceiptIndexViewModel
    {
        public PaginatedList<ReceiptDetailDto> Receipts { get; set; } = default!;

        public int? CurrentCustomerFilter { get; set; }
        public PaymentMode? CurrentPaymentModeFilter { get; set; }
        public string? CurrentFromDateFilter { get; set; }
        public string? CurrentToDateFilter { get; set; }
        public int CurrentPageSize { get; set; }

        public List<Customer>? Customers { get; set; }
        public SelectList? PaymentModes { get; set; }

        public int PageNumber { get; set; } = 1;
    }
}
