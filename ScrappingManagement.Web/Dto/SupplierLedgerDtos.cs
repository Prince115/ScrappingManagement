namespace ScrappingManagement.Web.Dto
{
	public class LedgerEntryDto
	{
		public DateTime Date { get; set; }
		public string Type { get; set; }
		public string Description { get; set; }
		public decimal Debit { get; set; }
		public decimal Credit { get; set; }
		public int DocumentId { get; set; }
		public bool IsWithGst{ get; set; }
		public string BillNo{ get; set; }
		public List<InvoiceItems>? Items { get; set; }
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
