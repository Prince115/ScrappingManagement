using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Dto
{
    public class ReceiptDetailDto
	{
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public decimal Amount { get; set; }
        public bool WithGst { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
