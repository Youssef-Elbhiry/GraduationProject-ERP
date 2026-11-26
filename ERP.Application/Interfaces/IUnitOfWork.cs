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
        ISalesOrderRepository SalesOrders { get; }
        IFulfillmentTaskRepository FulfillmentTasks { get; }
        IProductStockRepository ProductStocks { get; }
        IShipmentRepository Shipments { get; }
        public IGenericRepository<T> Repository<T>() where T : class;
        public Task<IDbContextTransaction> BeginTransactionAsync();
        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
        public Task<IDbContextTransaction> BeginSerializableTransactionAsync(CancellationToken cancellationToken);
        public Task CommitTransactionAsync();
        public Task<int> SaveChangesAsync();
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        public Task RollbackTransactionAsync();
    }
}
