using System.Security.Claims;
using ERP.Domain.Constants;
using ERP.Application.Customer;
using ERP.Application.Customer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api")]
public class CustomerController(ICustomerService customerService) : ControllerBase
{
    [HttpGet("customers")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken) =>
        Ok(await customerService.GetCustomersAsync(cancellationToken));

    [HttpGet("customers/{id}")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> GetCustomer(string id, CancellationToken cancellationToken)
    {
        var customer = await customerService.GetCustomerAsync(id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("customers/{id}/addresses")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> GetCustomerAddresses(string id, CancellationToken cancellationToken)
    {
        var addresses = await customerService.GetCustomerAddressesAsync(id, cancellationToken);
        return addresses is null ? NotFound() : Ok(addresses);
    }

    [HttpPost("customers/{id}/addresses")]
    [Authorize(Roles = UserRole.Sales)]
    public async Task<IActionResult> AddCustomerAddress(string id, CustomerAddressWriteDto dto, CancellationToken cancellationToken)
    {
        var address = await customerService.AddCustomerAddressAsync(id, dto, cancellationToken);
        return address is null ? NotFound("Customer not found.") : Ok(address);
    }

    [HttpGet("me/addresses")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> GetMyAddresses(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        return Ok(await customerService.GetMyAddressesAsync(userId, cancellationToken));
    }

    [HttpGet("me/addresses/{id:int}")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> GetMyAddress(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var address = await customerService.GetMyAddressAsync(userId, id, cancellationToken);
        return address is null ? NotFound("Address not found.") : Ok(address);
    }

    [HttpPost("me/addresses")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> AddMyAddress(CustomerAddressWriteDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var address = await customerService.AddMyAddressAsync(userId, dto, cancellationToken);
        return address is null ? NotFound("Customer not found.") : Ok(address);
    }

    [HttpPut("me/addresses/{id:int}")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> UpdateMyAddress(int id, CustomerAddressWriteDto dto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        var address = await customerService.UpdateMyAddressAsync(userId, id, dto, cancellationToken);
        return address is null ? NotFound("Address not found.") : Ok(address);
    }

    [HttpDelete("me/addresses/{id:int}")]
    [Authorize(Roles = UserRole.Customer)]
    public async Task<IActionResult> DeleteMyAddress(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();
        return await customerService.DeleteMyAddressAsync(userId, id, cancellationToken) ? NoContent() : NotFound("Address not found.");
    }
}
