using System.ComponentModel.DataAnnotations;

namespace ERP.Application.Auth.Dtos;

public class LoginUserDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
    [Required]
    public string Password { get; set; } = default!;
}
