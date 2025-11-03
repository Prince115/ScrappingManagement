namespace ScrappingManagement.Web.Dto
{

    public class CustomerLedgerDtos
	{
        public int? SelectedCustomerId { get; set; }
        public string SelectedCustomerName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<LedgerEntryDto> LedgerEntries { get; set; } = new List<LedgerEntryDto>();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal DueAmount { get; set; }
    }
}
