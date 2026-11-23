using ERP.Domain.Interfaces;
using ERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Infrastructure.Repositories;

public class DocumentNumberGenerator(ErpDbContext context) : IDocumentNumberGenerator
{
    public async Task<string> GenerateOrderNoAsync(CancellationToken cancellationToken = default)
    {
        var nextValue = await context.Database
            .SqlQueryRaw<long>("SELECT NEXT VALUE FOR dbo.OrderNumberSequence AS [Value]")
            .SingleAsync(cancellationToken);

        return $"SO-{DateTime.UtcNow.Year}-{nextValue:D6}";
    }
}
