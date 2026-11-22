using ERP.Application.Warehouse;
using ERP.Application.Warehouse.Dtos;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize(Roles = UserRole.Warehouse)]
public class WarehouseController(IWarehouseService warehouseService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(WarehouseWriteDto dto, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = warehouse.Id }, warehouse);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await warehouseService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.GetByIdAsync(id, cancellationToken);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, WarehouseWriteDto dto, CancellationToken cancellationToken)
    {
        var warehouse = await warehouseService.UpdateAsync(id, dto, cancellationToken);
        return warehouse is null ? NotFound() : Ok(warehouse);
    }
}
