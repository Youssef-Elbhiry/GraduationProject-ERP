using ERP.Application.Warehouse.Dtos;

namespace ERP.Application.Warehouse;

public interface IWarehouseService
{
    Task<WarehouseDto> CreateAsync(WarehouseWriteDto dto, CancellationToken cancellationToken = default);
    Task<List<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<WarehouseDto?> UpdateAsync(int id, WarehouseWriteDto dto, CancellationToken cancellationToken = default);
}
