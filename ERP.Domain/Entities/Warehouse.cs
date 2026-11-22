namespace ERP.Domain.Entities;

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Factory> Factories { get; set; } = new List<Factory>();
}
