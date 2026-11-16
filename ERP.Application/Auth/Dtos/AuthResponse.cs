using System.Text.Json.Serialization;

namespace ERP.Application.Auth.Dtos;

public class AuthResponse
{
    public string? Token { get; set; }

    public string ErrorMessage { get; set; } = default!;

    public bool IsAuthenticated { get; set; }

    public string Email { get; set; } = default!;

    // public string Name { get; set; } = default!;

    // public string Role { get; set; } = default!


    [JsonIgnore]
    public string? RefreshToken { get; set; }

    public DateTime RefreshTokenExpiration { get; set; }

}
