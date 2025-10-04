using System.ComponentModel.DataAnnotations;

namespace ScrappingManagement.Web.Models
{
	public class Product
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }

		public string Description { get; set; }
		public decimal Rate { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
}