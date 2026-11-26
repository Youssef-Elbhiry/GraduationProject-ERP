using ERP.Application.Interfaces;
using ERP.Infrastructure.Persistence;
using ERP.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ERP.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ErpDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new Dictionary<Type, object>();
        bool IsDispose = false;
        public UnitOfWork(ErpDbContext context)
        {
            _context = context;
            Customers = new CustomerRepository(context);
            Warehouses = new WarehouseRepository(context);
            Factories = new FactoryRepository(context);
            SalesOrders = new SalesOrderRepository(context);
            FulfillmentTasks = new FulfillmentTaskRepository(context);
            ProductStocks = new ProductStockRepository(context);
            Shipments = new ShipmentRepository(context);
        }

        public ICustomerRepository Customers { get; }
        public IWarehouseRepository Warehouses { get; }
        public IFactoryRepository Factories { get; }
        public ISalesOrderRepository SalesOrders { get; }
        public IFulfillmentTaskRepository FulfillmentTasks { get; }
        public IProductStockRepository ProductStocks { get; }
        public IShipmentRepository Shipments { get; }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();

        }

        public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
            _context.Database.BeginTransactionAsync(cancellationToken);
        public Task<IDbContextTransaction> BeginSerializableTransactionAsync(
            CancellationToken cancellationToken)
        {
            return _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable,
                cancellationToken);
        }

        public async Task CommitTransactionAsync()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public void Dispose()
        {
            if (!IsDispose)
            {
                _context.Dispose();
                GC.SuppressFinalize(this);
                IsDispose = true;
            }
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return _repositories[typeof(T)] as IGenericRepository<T>;
            }
            var repository = new GenericRepository<T>(_context);
            _repositories[typeof(T)] = repository;
            return repository;
        }

        public async Task RollbackTransactionAsync()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
            _context.SaveChangesAsync(cancellationToken);
    }
}
