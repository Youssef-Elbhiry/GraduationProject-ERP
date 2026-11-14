using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Category> CreateCategoryAsync(Category category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await GetCategoryByIdAsync(id);
            if (category == null)
            {
                return false;
            }

             _unitOfWork.Repository<Category>().Delete(category);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _unitOfWork.Repository<Category>().GetAsListAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Category>().GetByIdAsync(id);
        }
        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            var existingCategory = await GetCategoryByIdAsync(category.Id);
            if(existingCategory == null)
            {
                throw new InvalidOperationException("Category not found");
            }

            existingCategory.Name = category.Name;

            await _unitOfWork.SaveChangesAsync();

            return existingCategory;
        }
        public async Task<bool> IsCategoryNameExistsAsync(string name, int? categoryId = null)
        {
            var query = _unitOfWork.Repository<Category>().GetAsQuerable();
            var isExists = false;
            if (categoryId != null)
            {
                isExists = query.Any(c => c.Name == name && c.Id != categoryId);
            }
            else
            {
                isExists = query.Any(c => c.Name == name);
            }
            return isExists;
        }
    }
}
