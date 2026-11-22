namespace ERP.Domain.Entities;

public class Factory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int WarehouseId { get; set; }
    public bool IsActive { get; set; } = true;
    public Warehouse Warehouse { get; set; } = null!;
}
