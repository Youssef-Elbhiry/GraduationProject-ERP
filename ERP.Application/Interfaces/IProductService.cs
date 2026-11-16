using ERP.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERP.Application.Interfaces
{
    public interface IProductService
    {
        public Task<List<Product>> GetAllProductsAsync(bool onlyActive = false ,bool includeCategories = false);
        public Task<Product> GetProductByIdAsync(int id, bool includeCategories = false);
        public Task<bool> CreateProductAsync(Product product,IFormFile file);
        public Task<bool> UpdateProductAsync(Product product, IFormFile? file);
        public Task<bool> DeleteProductAsync(int id);
        public Task<bool> IsProductCodeExistsAsync(string code, int? productId = null);

    }
}
