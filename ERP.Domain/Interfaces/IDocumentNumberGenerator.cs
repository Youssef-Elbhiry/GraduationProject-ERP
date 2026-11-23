namespace ERP.Domain.Interfaces;

public interface IDocumentNumberGenerator
{
    Task<string> GenerateOrderNoAsync(CancellationToken cancellationToken = default);
}
