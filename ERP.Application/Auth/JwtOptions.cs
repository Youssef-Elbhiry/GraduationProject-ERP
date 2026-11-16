namespace ERP.Application.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SignInKey { get; set; } = default!;

    public double DurationInMinutes { get; set; }
}
