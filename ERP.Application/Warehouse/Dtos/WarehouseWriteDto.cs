using System.ComponentModel.DataAnnotations;

namespace ERP.Application.Warehouse.Dtos;

public class WarehouseWriteDto
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}
