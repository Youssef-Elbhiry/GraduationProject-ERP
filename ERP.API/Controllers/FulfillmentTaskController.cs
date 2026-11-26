using ERP.Application.ProductStock;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/fulfillment-tasks")]
[Authorize(Roles = UserRole.Warehouse)]
public class FulfillmentTaskController(IProductStockService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] FulfillmentStatus? status,
        CancellationToken cancellationToken)
    {
        var tasks = await service.GetTasksAsync(status, cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await service.GetTaskAsync(id, cancellationToken);
        if (task is null)
        {
            return NotFound("Fulfillment task not found.");
        }

        return Ok(task);
    }

    [HttpPost("{id:int}/check-availability")]
    public async Task<IActionResult> Check(int id, CancellationToken cancellationToken)
    {
        var result = await service.CheckAvailabilityAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return StatusCode(result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }
}
