using Microsoft.AspNetCore.Identity;

namespace ERP.Domain.Entities;

public class AppUser : IdentityUser
{
    // public string Name { get; set; } = default!;

    public List<RefreshToken>? RefreshTokens { get; set; }
}
