using System.ComponentModel.DataAnnotations;

namespace TestProject.ViewModels_DTOs_
{
    public class RegisterUserDTO
    {
        public required string UserName { get; set; }
        
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public required string ConfirmPassword { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format. Must be like example@hello.com")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        ErrorMessage = "Email must be in format: example@hello.com")]
        public required string Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
