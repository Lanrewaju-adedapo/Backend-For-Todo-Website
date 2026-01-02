using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestProject.Models.Authentication;

namespace TestProject.Models.Entities
{
    public class Tasks
    {
        [Key]
        public int TaskId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public byte? Priority { get; set; } // 0: Low, 1: Medium, 2: High
        public bool? IsCompleted { get; set; } 
        public DateTime? CreatedAt { get; set; }
        public DateTime? LastModified { get; set; }

        public int? CategoryId { get; set; }

        [Column("user_id")]
        public int? User_id { get; set; }
        public virtual Users User { get; set; }

    }
}
