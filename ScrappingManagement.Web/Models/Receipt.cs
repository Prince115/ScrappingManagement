using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.Web.Models
{
	public class Receipt
	{
		public int Id { get; set; }

		[Required]
		public int CustomerId { get; set; }
		[Required]
		public PaymentMode PaymentMode { get; set; }

		[Required]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal Amount { get; set; }

		public bool WithGst { get; set; } = false;

		public string? Description { get; set; }

		[Required]
		public DateTime Date { get; set; } = DateTime.Now;
	}
}
