using System.ComponentModel.DataAnnotations;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Dto
{
    public class CreatePaymentDto
    {
        public int SupplierId { get; set; }

        [Required]
        public PaymentMode PaymentMode { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
