using System.Security.Claims;
using ERP.Application.ProductStock;
using ERP.Application.ProductStock.Dtos;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize(Roles = UserRole.Warehouse)]
public class ShipmentController(ShipmentService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        ShipmentCreateDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.CreateAsync(dto, userId, cancellationToken);
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return CreatedAtAction(
            nameof(Get),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPatch("{id:int}/delivered")]
    public async Task<IActionResult> Delivered(int id, CancellationToken cancellationToken)
    {
        var result = await service.DeliveredAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken cancellationToken)
    {
        var shipment = await service.GetAsync(id, cancellationToken);
        if (shipment is null)
        {
            return NotFound("Shipment not found.");
        }

        return Ok(shipment);
    }
}
