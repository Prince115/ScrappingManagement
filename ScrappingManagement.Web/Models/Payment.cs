using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.Web.Models
{
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [Required]
        public PaymentMode PaymentMode { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateOnly Date { get; set; }
    }
}
