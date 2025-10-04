using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Dto
{
    public class PaymentDetailDto
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
    }
}
