using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestProject.Models.Authentication;

namespace TestProject.Models.Entities
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ColorCode { get; set; }

        [Column("user_id")]
        public int? User_id { get; set; }
        public virtual Users User { get; set; }
    }
}
