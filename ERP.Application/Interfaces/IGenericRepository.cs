using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        public IQueryable<T> GetAsQuerable();
        public Task<T> GetByIdAsync(int Id);
        public Task<List<T>> GetAsListAsync();
        public Task AddAsync(T entity);
        public void Delete(T entity);
        public void Update(T entity);
        public Task AddRangeAsync(IEnumerable<T> entities);
        public void RemoveRange(IEnumerable<T> entities);
    }
}
