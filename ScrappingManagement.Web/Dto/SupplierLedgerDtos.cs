namespace ScrappingManagement.Web.Dto
{
    public class LedgerEntryDto
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } // "Quote" or "Payment"
        public string Description { get; set; }
        public decimal Debit { get; set; } // Amount for Quotes
        public decimal Credit { get; set; } // Amount for Payments
        public int DocumentId { get; set; } // Id of the Quote or Payment
    }

    public class SupplierLedgerDto
    {
        public int? SelectedSupplierId { get; set; }
        public string SelectedSupplierName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<LedgerEntryDto> LedgerEntries { get; set; } = new List<LedgerEntryDto>();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal DueAmount { get; set; }
    }
}
