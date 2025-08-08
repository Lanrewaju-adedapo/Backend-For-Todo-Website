using System.ComponentModel.DataAnnotations;

namespace TestProject.Models.Entities
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ColorCode { get; set; }
    }
}
