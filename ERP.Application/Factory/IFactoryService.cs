using ERP.Application.Factory.Dtos;

namespace ERP.Application.Factory;

public interface IFactoryService
{
    Task<FactoryDto?> CreateAsync(FactoryWriteDto dto, CancellationToken cancellationToken = default);
    Task<List<FactoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FactoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FactoryDto?> UpdateAsync(int id, FactoryWriteDto dto, CancellationToken cancellationToken = default);
}
