using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TestProject.Models.Entities;

namespace TestProject.Models.Authentication
{
    [Table("users")]
    public class Users : IdentityUser<int>
    {


        [StringLength(50)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [StringLength(50)]
        [Column("last_name")]
        public string LastName { get; set; }

        [StringLength(50)]
        [Column("timezone")]
        public string Timezone { get; set; } = "UTC";

        [Column("email_verified")]
        public bool EmailVerified { get; set; } = false;

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } 

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [StringLength(500)]
        [Column("refresh_token")]
        public string? RefreshToken { get; set; }

        [Column("refresh_token_expiry")]
        public DateTime? RefreshTokenExpiry { get; set; } 
        public virtual ICollection<Tasks> Tasks { get; set; }
        public virtual ICollection<Categories> Categories { get; set; }

        public Users()
        {
            Tasks = new HashSet<Tasks>();
            Categories = new HashSet<Categories>();
        }
    }
}