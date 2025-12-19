namespace ScrappingManagement.Web.Dto
{

	public class CustomerLedgerDtos
	{
		public int? CustomerId { get; set; }
		public string CustomerName { get; set; }
		public DateOnly? FromDate { get; set; }
		public DateOnly? ToDate { get; set; }
		public List<LedgerEntryDto> LedgerEntries { get; set; } = new List<LedgerEntryDto>();
		public decimal TotalDebit { get; set; }
		public decimal TotalCredit { get; set; }
		public decimal DueAmount { get; set; }
		public decimal OpeningBalance { get; set; }
	}
}
