using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.Web.Models
{
	public class QuoteProduct
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int QuoteId { get; set; }
		public double? LoadedWeight { get; set; } = 0;
		public double? UnloadWeight { get; set; } = 0;

		public string? Description { get; set; } = "";
		public double Gross { get; set; }
		[Column("nos")]
		public int? Nos { get; set; }

		public int BoraCount { get; set; } = 0;
		public double BoraReport { get; set; } = 0;
		public double ProductReport { get; set; } = 0;
		[Column("netweight")]
		public double NetWeight { get; set; }
		public double Rate { get; set; }
		[NotMapped]
		public int Deleted { get; set; } = 0;

		public double TotalAmount { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

}
