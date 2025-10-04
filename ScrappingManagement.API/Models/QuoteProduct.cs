namespace ScrappingManagement.API.Models
{
    public class QuoteProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public double LoadedWeight { get; set; }
        public double UnloadWeight { get; set; }

        public double Gross => LoadedWeight - UnloadWeight;

        public int BoraCount { get; set; }
        public double BoraReport { get; set; }
        public double ProductReport { get; set; }

        public double NetWeight => Gross - (BoraCount * BoraReport) - ProductReport;
        public double Rate { get; set; }

        public double TotalAmount => NetWeight * Rate;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
