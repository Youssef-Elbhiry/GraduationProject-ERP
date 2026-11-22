using ERP.Application.Interfaces;
using ERP.Application.Warehouse.Dtos;
using ERP.Domain.Entities;

namespace ERP.Application.Warehouse;

public class WarehouseService(IUnitOfWork unitOfWork) : IWarehouseService
{
    public async Task<WarehouseDto> CreateAsync(WarehouseWriteDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var entity = new ERP.Domain.Entities.Warehouse { Name = dto.Name.Trim(), Address = Clean(dto.Address), IsActive = dto.IsActive };
        await unitOfWork.Warehouses.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<List<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await unitOfWork.Warehouses.GetAllAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<WarehouseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : ToDto(entity);
    }

    public async Task<WarehouseDto?> UpdateAsync(int id, WarehouseWriteDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await unitOfWork.Warehouses.GetByIdAsync(id, cancellationToken);
        if (entity is null) return null;
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        entity.Name = dto.Name.Trim(); entity.Address = Clean(dto.Address); entity.IsActive = dto.IsActive;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToDto(entity);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static WarehouseDto ToDto(ERP.Domain.Entities.Warehouse x) => new() { Id = x.Id, Name = x.Name, Address = x.Address, IsActive = x.IsActive };
}
