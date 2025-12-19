using System.ComponentModel.DataAnnotations;
using ScrappingManagement.Web.Models;

namespace ScrappingManagement.Web.Dto
{
    public class EditReceiptDto
	{
        [Required]
        public int Id { get; set; }

        public int CustomerId { get; set; }

        [Required]
        public PaymentMode PaymentMode { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateOnly Date { get; set; }
    }
}
