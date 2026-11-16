using System.ComponentModel.DataAnnotations;

namespace ERP.Application.Auth.Dtos
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]

        public string Email { get; set; } = default!;
        [Required]
        [RegularExpression(@"^01[2150]\d{8}",ErrorMessage ="The Phone Number must be egyption Number")]
        public string PhoneNumber { get; set; } = default!;
        [Required]
        public string Password { get; set; } = default!;
        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = default!;
    }
}
