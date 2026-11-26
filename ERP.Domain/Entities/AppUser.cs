using Microsoft.AspNetCore.Identity;

namespace ERP.Domain.Entities;

public class AppUser : IdentityUser
{
    public string Name { get; set; } = string.Empty;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
