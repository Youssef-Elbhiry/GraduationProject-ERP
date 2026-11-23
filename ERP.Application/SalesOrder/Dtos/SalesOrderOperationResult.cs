namespace ERP.Application.SalesOrder.Dtos;

public enum SalesOrderFailureKind
{
    BadRequest,
    NotFound,
    Conflict
}

public class SalesOrderOperationResult<T> where T : class
{
    public T? Value { get; private init; }
    public SalesOrderFailureKind? FailureKind { get; private init; }
    public string? Error { get; private init; }

    public static SalesOrderOperationResult<T> Success(T value) => new() { Value = value };
    public static SalesOrderOperationResult<T> BadRequest(string error) => new() { FailureKind = SalesOrderFailureKind.BadRequest, Error = error };
    public static SalesOrderOperationResult<T> NotFound(string error = "Sales order not found.") => new() { FailureKind = SalesOrderFailureKind.NotFound, Error = error };
    public static SalesOrderOperationResult<T> Conflict(string error) => new() { FailureKind = SalesOrderFailureKind.Conflict, Error = error };
}
