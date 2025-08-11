using System.ComponentModel.DataAnnotations;

namespace ScrappingManagement.API.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }

}
