using ERP.Application.Factory.Dtos;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;

namespace ERP.Application.Factory;

public class FactoryService(IUnitOfWork unitOfWork) : IFactoryService
{
    public async Task<FactoryDto?> CreateAsync(FactoryWriteDto dto, CancellationToken cancellationToken = default)
    {
        if (!await unitOfWork.Factories.WarehouseExistsAsync(dto.WarehouseId, cancellationToken)) return null;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var entity = new ERP.Domain.Entities.Factory { Name = dto.Name.Trim(), Address = Clean(dto.Address), WarehouseId = dto.WarehouseId, IsActive = dto.IsActive };
        await unitOfWork.Factories.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<List<FactoryDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await unitOfWork.Factories.GetAllAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<FactoryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Factories.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<FactoryDto?> UpdateAsync(int id, FactoryWriteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Factories.GetByIdAsync(id, cancellationToken);
        if (entity is null || !await unitOfWork.Factories.WarehouseExistsAsync(dto.WarehouseId, cancellationToken)) return null;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        entity.Name = dto.Name.Trim(); entity.Address = Clean(dto.Address);
        entity.WarehouseId = dto.WarehouseId; entity.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static FactoryDto ToDto(ERP.Domain.Entities.Factory x) => new()
    {
        Id = x.Id, Name = x.Name, Address = x.Address, WarehouseId = x.WarehouseId,
        WarehouseName = x.Warehouse?.Name, IsActive = x.IsActive
    };
}
