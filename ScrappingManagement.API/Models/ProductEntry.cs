using System.ComponentModel.DataAnnotations.Schema;

namespace ScrappingManagement.API.Models
{
    public class ProductEntry
    {
        public int Id { get; set; }
        public string Product { get; set; } = string.Empty;
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
        [ForeignKey("CustomerTransactionId")]
        public CustomerTransaction? CustomerTransaction { get; set; }
    }

}
