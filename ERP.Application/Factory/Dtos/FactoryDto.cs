namespace ERP.Application.Factory.Dtos;

public class FactoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public bool IsActive { get; set; }
}
