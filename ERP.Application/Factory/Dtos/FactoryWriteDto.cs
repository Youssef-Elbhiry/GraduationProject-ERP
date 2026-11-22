using System.ComponentModel.DataAnnotations;

namespace ERP.Application.Factory.Dtos;

public class FactoryWriteDto
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string? Address { get; set; }
    [Range(1, int.MaxValue)]
    public int WarehouseId { get; set; }
    public bool IsActive { get; set; } = true;
}
