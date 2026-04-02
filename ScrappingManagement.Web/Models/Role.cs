using System.ComponentModel.DataAnnotations;

namespace ScrappingManagement.Web.Models
{
    public class Role
    {
        public string Id { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
