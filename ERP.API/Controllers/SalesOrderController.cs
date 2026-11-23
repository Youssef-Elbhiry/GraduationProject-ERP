using System.Security.Claims;
using ERP.Application.SalesOrder;
using ERP.Application.SalesOrder.Dtos;
using ERP.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/sales-orders")]
public class SalesOrderController(ISalesOrderService salesOrderService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> Create(SalesOrderWriteDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var result = await salesOrderService.CreateAsync(dto, userId, cancellationToken);
        return ToActionResult(result, CreatedAtAction(nameof(GetById), new { id = result.Value?.Id }, result.Value));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> Update(int id, SalesOrderWriteDto dto, CancellationToken cancellationToken)
    {
        var result = await salesOrderService.UpdateAsync(id, dto, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet]
    [Authorize(Roles = UserRole.Sales + "," + UserRole.Warehouse + "," + UserRole.Accountant)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? customerUserId,
        [FromQuery] SalesOrderStatus? status,
        CancellationToken cancellationToken) =>
        Ok(await salesOrderService.GetAllAsync(customerUserId, status, cancellationToken));

    [HttpGet("{id:int}")]
    [Authorize(Roles = UserRole.Sales + "," + UserRole.Warehouse + "," + UserRole.Accountant)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await salesOrderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound("Sales order not found.") : Ok(order);
    }

    [HttpPost("{id:int}/send")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> Send(int id, CancellationToken cancellationToken)
    {
        var result = await salesOrderService.SendAsync(id, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("~/api/me/sales-orders")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        return Ok(await salesOrderService.GetMyOrdersAsync(userId, cancellationToken));
    }

    [HttpGet("~/api/me/sales-orders/{id:int}")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> GetMyOrder(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var order = await salesOrderService.GetMyOrderAsync(userId, id, cancellationToken);
        return order is null ? NotFound("Sales order not found.") : Ok(order);
    }

    [HttpPost("{id:int}/confirm")]
    [Authorize(Roles = UserRole.Customer + "," + UserRole.Sales)]
    public async Task<IActionResult> Confirm(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var result = await salesOrderService.ConfirmAsync(id, userId, User.IsInRole(UserRole.Sales), cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> Reject(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var result = await salesOrderService.RejectAsync(id, userId, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken) =>
        ToActionResult(await salesOrderService.CancelAsync(id, cancellationToken));

    private IActionResult ToActionResult<T>(SalesOrderOperationResult<T> result, IActionResult? successResult = null) where T : class
    {
        if (result.Value is not null) return successResult ?? Ok(result.Value);
        return result.FailureKind switch
        {
            SalesOrderFailureKind.NotFound => NotFound(result.Error),
            SalesOrderFailureKind.Conflict => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }
}
