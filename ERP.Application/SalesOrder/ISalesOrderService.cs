using ERP.Application.SalesOrder.Dtos;
using ERP.Domain.Constants;

namespace ERP.Application.SalesOrder;

public interface ISalesOrderService
{
    Task<SalesOrderOperationResult<SalesOrderDto>> CreateAsync(SalesOrderWriteDto dto, string createdByUserId, CancellationToken cancellationToken = default);
    Task<SalesOrderOperationResult<SalesOrderDto>> UpdateAsync(int id, SalesOrderWriteDto dto, CancellationToken cancellationToken = default);
    Task<List<SalesOrderDto>> GetAllAsync(string? customerUserId, SalesOrderStatus? status, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<SalesOrderDto>> GetMyOrdersAsync(string customerUserId, CancellationToken cancellationToken = default);
    Task<SalesOrderDto?> GetMyOrderAsync(string customerUserId, int id, CancellationToken cancellationToken = default);
    Task<SalesOrderOperationResult<SendSalesOrderResultDto>> SendAsync(int id, CancellationToken cancellationToken = default);
    Task<SalesOrderOperationResult<SalesOrderDto>> ConfirmAsync(int id, string callerUserId, bool isSales, CancellationToken cancellationToken = default);
    Task<SalesOrderOperationResult<SalesOrderDto>> RejectAsync(int id, string callerUserId, CancellationToken cancellationToken = default);
    Task<SalesOrderOperationResult<SalesOrderDto>> CancelAsync(int id, CancellationToken cancellationToken = default);
}
