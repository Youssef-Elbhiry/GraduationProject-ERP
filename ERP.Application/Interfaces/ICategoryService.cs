using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface ICategoryService
    {
        public Task<List<Category>> GetAllCategoriesAsync();
        public Task<Category> GetCategoryByIdAsync(int id);
        public Task<Category> CreateCategoryAsync(Category category);
        public Task<Category> UpdateCategoryAsync(Category category);
        public Task<bool> DeleteCategoryAsync(int id);
        public Task<bool> IsCategoryNameExistsAsync(string name, int? categoryId = null);
    }
}
