using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ERP.Domain.Repositories;

namespace ERP.Application.Interfaces
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IWarehouseRepository Warehouses { get; }
        IFactoryRepository Factories { get; }
        public IGenericRepository<T> Repository<T>() where T : class;
        public Task<IDbContextTransaction> BeginTransactionAsync();
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
        public Task CommitTransactionAsync();
        public Task<int> SaveChangesAsync();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        public Task RollbackTransactionAsync();
    }
}
