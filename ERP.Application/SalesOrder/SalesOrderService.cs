using ERP.Application.Interfaces;
using ERP.Application.SalesOrder.Dtos;
using ERP.Domain.Constants;
using ERP.Domain.Entities;
using ERP.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using SalesOrderEntity = ERP.Domain.Entities.SalesOrder;

namespace ERP.Application.SalesOrder;

public class SalesOrderService(
    IUnitOfWork unitOfWork,
    IDocumentNumberGenerator documentNumberGenerator,
    IEmailSender emailSender,
    ILogger<SalesOrderService> logger) : ISalesOrderService
{
    public async Task<SalesOrderOperationResult<SalesOrderDto>> CreateAsync(
        SalesOrderWriteDto dto, string createdByUserId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var validationError = await ValidateCustomerAndAddressAsync(dto, cancellationToken);
        if (validationError is not null) return SalesOrderOperationResult<SalesOrderDto>.BadRequest(validationError);

        var (lines, linesError) = await BuildLinesAsync(dto.Lines, cancellationToken);
        if (linesError is not null) return SalesOrderOperationResult<SalesOrderDto>.BadRequest(linesError);

        var order = new SalesOrderEntity
        {
            OrderNo = await documentNumberGenerator.GenerateOrderNoAsync(cancellationToken),
            CustomerUserId = dto.CustomerUserId,
            ShippingAddressId = dto.ShippingAddressId,
            Status = SalesOrderStatus.Draft,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow,
            ValidUntil = dto.ValidUntil,
            Lines = lines!
        };
        await unitOfWork.SalesOrders.AddAsync(order, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return SalesOrderOperationResult<SalesOrderDto>.Success(ToDto(order));
    }

    public async Task<SalesOrderOperationResult<SalesOrderDto>> UpdateAsync(
        int id, SalesOrderWriteDto dto, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
        if (order is null) return SalesOrderOperationResult<SalesOrderDto>.NotFound();
        if (order.Status != SalesOrderStatus.Draft)
            return SalesOrderOperationResult<SalesOrderDto>.Conflict("Only Draft sales orders can be edited.");

        var validationError = await ValidateCustomerAndAddressAsync(dto, cancellationToken);
        if (validationError is not null) return SalesOrderOperationResult<SalesOrderDto>.BadRequest(validationError);

        var (lines, linesError) = await BuildLinesAsync(dto.Lines, cancellationToken);
        if (linesError is not null) return SalesOrderOperationResult<SalesOrderDto>.BadRequest(linesError);

        order.CustomerUserId = dto.CustomerUserId;
        order.ShippingAddressId = dto.ShippingAddressId;
        order.ValidUntil = dto.ValidUntil;
        unitOfWork.SalesOrders.ReplaceLines(order, lines!);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return SalesOrderOperationResult<SalesOrderDto>.Success(ToDto(order));
    }

    public async Task<List<SalesOrderDto>> GetAllAsync(
        string? customerUserId, SalesOrderStatus? status, CancellationToken cancellationToken = default) =>
        (await unitOfWork.SalesOrders.GetAllAsync(customerUserId, status, cancellationToken)).Select(ToDto).ToList();

    public async Task<SalesOrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
        return order is null ? null : ToDto(order);
    }

    public async Task<List<SalesOrderDto>> GetMyOrdersAsync(string customerUserId, CancellationToken cancellationToken = default) =>
        (await unitOfWork.SalesOrders.GetAllAsync(customerUserId, null, cancellationToken))
            .Where(x => x.Status != SalesOrderStatus.Draft).Select(ToDto).ToList();

    public async Task<SalesOrderDto?> GetMyOrderAsync(string customerUserId, int id, CancellationToken cancellationToken = default)
    {
        var order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
        return order is null || order.CustomerUserId != customerUserId || order.Status == SalesOrderStatus.Draft
            ? null
            : ToDto(order);
    }

    public async Task<SalesOrderOperationResult<SendSalesOrderResultDto>> SendAsync(int id, CancellationToken cancellationToken = default)
    {
        SalesOrderEntity? order;
        await using (var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken))
        {
            order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
            if (order is null) return SalesOrderOperationResult<SendSalesOrderResultDto>.NotFound();
            if (order.Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Sent))
                return SalesOrderOperationResult<SendSalesOrderResultDto>.Conflict("Only Draft or Sent sales orders can be sent.");
            if (order.Status == SalesOrderStatus.Draft && order.Lines.Count == 0)
                return SalesOrderOperationResult<SendSalesOrderResultDto>.BadRequest("At least one line is required before sending.");

            if (order.Status == SalesOrderStatus.Draft)
            {
                order.Status = SalesOrderStatus.Sent;
                order.SentAt = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        }

        var emailSent = false;
        try
        {
            if (string.IsNullOrWhiteSpace(order!.CustomerUser.Email))
                throw new InvalidOperationException("The customer does not have an email address.");

            var body = $"Sales order {order.OrderNo} is ready for your review. Valid until {order.ValidUntil:yyyy-MM-dd}. Total: {CalculateTotal(order):0.00}.";
            await emailSender.SendAsync(order.CustomerUser.Email, $"Sales order {order.OrderNo}", body, cancellationToken);
            emailSent = true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Email delivery failed for sales order {SalesOrderId} ({OrderNo})", order!.Id, order.OrderNo);
        }

        return SalesOrderOperationResult<SendSalesOrderResultDto>.Success(new SendSalesOrderResultDto
        {
            Order = ToDto(order!),
            EmailSent = emailSent
        });
    }

    public async Task<SalesOrderOperationResult<SalesOrderDto>> ConfirmAsync(
        int id, string callerUserId, bool isSales, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
        if (order is null || (!isSales && order.CustomerUserId != callerUserId))
            return SalesOrderOperationResult<SalesOrderDto>.NotFound();

        if (order.Status == SalesOrderStatus.Confirmed)
            return SalesOrderOperationResult<SalesOrderDto>.Success(ToDto(order));
        if (order.Status != SalesOrderStatus.Sent)
            return SalesOrderOperationResult<SalesOrderDto>.Conflict("Only Sent sales orders can be confirmed.");
        if (order.ValidUntil < DateTime.UtcNow)
            return SalesOrderOperationResult<SalesOrderDto>.Conflict("This sales order has expired.");

        var warehouse = await unitOfWork.Warehouses.GetFirstActiveAsync(cancellationToken);
        if (warehouse is null)
            return SalesOrderOperationResult<SalesOrderDto>.Conflict("No active warehouse is configured.");

        // TODO (Phase 3): Run the order availability check here before creating the fulfillment task.
        order.Status = SalesOrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await unitOfWork.FulfillmentTasks.AddAsync(new FulfillmentTask
        {
            SalesOrderId = order.Id,
            WarehouseId = warehouse.Id,
            Status = FulfillmentStatus.Waiting,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return SalesOrderOperationResult<SalesOrderDto>.Success(ToDto(order));
    }

    public async Task<SalesOrderOperationResult<SalesOrderDto>> RejectAsync(
        int id, string callerUserId, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
        if (order is null || order.CustomerUserId != callerUserId)
            return SalesOrderOperationResult<SalesOrderDto>.NotFound();
        if (order.Status != SalesOrderStatus.Sent)
            return SalesOrderOperationResult<SalesOrderDto>.Conflict("Only Sent sales orders can be rejected.");

        order.Status = SalesOrderStatus.Rejected;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return SalesOrderOperationResult<SalesOrderDto>.Success(ToDto(order));
    }

    public async Task<SalesOrderOperationResult<SalesOrderDto>> CancelAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        var order = await unitOfWork.SalesOrders.GetByIdAsync(id, cancellationToken);
        if (order is null) return SalesOrderOperationResult<SalesOrderDto>.NotFound();
        if (order.Status is not (SalesOrderStatus.Draft or SalesOrderStatus.Sent or SalesOrderStatus.Confirmed))
            return SalesOrderOperationResult<SalesOrderDto>.Conflict("Only Draft, Sent, or Confirmed sales orders can be cancelled.");

        var fulfillmentTask = await unitOfWork.FulfillmentTasks.GetBySalesOrderIdAsync(id, cancellationToken);
        if (fulfillmentTask is not null) fulfillmentTask.Status = FulfillmentStatus.Cancelled;
        order.Status = SalesOrderStatus.Cancelled;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return SalesOrderOperationResult<SalesOrderDto>.Success(ToDto(order));
    }

    private async Task<string?> ValidateCustomerAndAddressAsync(SalesOrderWriteDto dto, CancellationToken cancellationToken)
    {
        if (await unitOfWork.Customers.GetActiveCustomerByIdAsync(dto.CustomerUserId, cancellationToken) is null)
            return "Customer must be an active user with the Customer role.";

        var address = await unitOfWork.Customers.GetAddressAsync(dto.ShippingAddressId, cancellationToken);
        return address is null || address.CustomerUserId != dto.CustomerUserId
            ? "Shipping address must belong to the selected customer."
            : null;
    }

    private async Task<(List<SalesOrderLine>? Lines, string? Error)> BuildLinesAsync(
        IEnumerable<SalesOrderLineInputDto> lineDtos, CancellationToken cancellationToken)
    {
        var inputs = lineDtos.ToList();
        if (inputs.Any(x => x.Qty <= 0)) return (null, "Line quantities must be greater than zero.");
        if (inputs.Any(x => x.ProductId <= 0)) return (null, "ProductId must be greater than zero.");
        if (inputs.Count == 0) return (new List<SalesOrderLine>(), null);

        var productIds = inputs.Select(x => x.ProductId).Distinct().ToList();
        var products = await unitOfWork.Repository<Product>().GetAsQuerable()
            .Where(x => productIds.Contains(x.Id)).ToListAsync(cancellationToken);
        var productsById = products.ToDictionary(x => x.Id);
        if (productsById.Count != productIds.Count || products.Any(x => !x.IsActive))
            return (null, "Every product must exist and be active.");

        return (inputs.Select(x => new SalesOrderLine
        {
            ProductId = x.ProductId,
            Qty = x.Qty,
            UnitPrice = productsById[x.ProductId].SalesPrice
        }).ToList(), null);
    }

    private static SalesOrderDto ToDto(SalesOrderEntity order)
    {
        var now = DateTime.UtcNow;
        var lines = order.Lines.Select(x => new SalesOrderLineDto
        {
            Id = x.Id,
            ProductId = x.ProductId,
            Qty = x.Qty,
            UnitPrice = x.UnitPrice,
            LineTotal = x.UnitPrice * x.Qty
        }).ToList();

        return new SalesOrderDto
        {
            Id = order.Id,
            OrderNo = order.OrderNo,
            CustomerUserId = order.CustomerUserId,
            ShippingAddressId = order.ShippingAddressId,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            ValidUntil = order.ValidUntil,
            SentAt = order.SentAt,
            ConfirmedAt = order.ConfirmedAt,
            Lines = lines,
            Total = lines.Sum(x => x.LineTotal),
            IsExpired = order.Status == SalesOrderStatus.Sent && order.ValidUntil < now,
            DeliveryState = null,
            InvoiceState = null,
            PaymentState = null
        };
    }

    private static decimal CalculateTotal(SalesOrderEntity order) => order.Lines.Sum(x => x.UnitPrice * x.Qty);
}
