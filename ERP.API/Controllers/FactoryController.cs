using ERP.Application.Factory;
using ERP.Application.Factory.Dtos;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/factories")]
[Authorize(Roles = UserRole.Warehouse)]
public class FactoryController(IFactoryService factoryService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(FactoryWriteDto dto, CancellationToken cancellationToken)
    {
        var factory = await factoryService.CreateAsync(dto, cancellationToken);
        return factory is null ? BadRequest("Warehouse does not exist.") : CreatedAtAction(nameof(GetById), new { id = factory.Id }, factory);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await factoryService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var factory = await factoryService.GetByIdAsync(id, cancellationToken);
        return factory is null ? NotFound() : Ok(factory);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FactoryWriteDto dto, CancellationToken cancellationToken)
    {
        if (await factoryService.GetByIdAsync(id, cancellationToken) is null) return NotFound();
        var factory = await factoryService.UpdateAsync(id, dto, cancellationToken);
        return factory is null ? BadRequest("Factory or warehouse does not exist.") : Ok(factory);
    }
}
