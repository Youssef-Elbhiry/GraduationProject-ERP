namespace ERP.Domain.Entities;

public class CustomerAddress
{
    public int Id { get; set; }
    public string CustomerUserId { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string? Line2 { get; set; }
    public string City { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public AppUser CustomerUser { get; set; } = null!;
}
