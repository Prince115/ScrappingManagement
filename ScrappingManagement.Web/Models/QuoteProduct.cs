namespace ScrappingManagement.Web.Models
{
	public class QuoteProduct
	{
		public int Id { get; set; }
		public int ProductId { get; set; }
		public int QuoteId { get; set; }
		public double? LoadedWeight { get; set; } = 0;
		public double? UnloadWeight { get; set; } = 0;

		public double Gross { get; set; }
		public int? Nos { get; set; }  

		public int BoraCount { get; set; } = 0;
		public double BoraReport { get; set; } = 0;
		public double ProductReport { get; set; } = 0;

		public double NetWeight => Gross - BoraReport - ProductReport;
		public double Rate { get; set; }

		public double TotalAmount { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}

}
