using System.Security.Claims;
using ERP.Application.ProductStock;
using ERP.Application.ProductStock.Dtos;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/product-stock")]
[Authorize(Roles = UserRole.Warehouse)]
public class ProductStockController(IProductStockService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int productId,
        [FromQuery] int warehouseId,
        CancellationToken cancellationToken)
    {
        if (productId <= 0 || warehouseId <= 0)
        {
            return BadRequest("productId and warehouseId are required.");
        }

        var stock = await service.GetAsync(productId, warehouseId, cancellationToken);
        return Ok(stock);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> Movements(
        [FromQuery] int? productId,
        [FromQuery] int? warehouseId,
        CancellationToken cancellationToken)
    {
        var movements = await service.GetMovementsAsync(
            productId,
            warehouseId,
            cancellationToken);

        return Ok(movements);
    }

    [HttpPost("adjustments")]
    public async Task<IActionResult> Adjust(
        StockAdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await service.AdjustAsync(dto, userId, cancellationToken);
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }
}
