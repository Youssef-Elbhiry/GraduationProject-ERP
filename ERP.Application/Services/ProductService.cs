using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace ERP.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, IFileService fileService, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<bool> CreateProductAsync(Product product, IFormFile file)
        {
            string? imageUrl = null;
            try
            {
                if (product == null || file == null || file.Length == 0)
                {
                    return false;
                }

                imageUrl = await _fileService.UploadFileAsync(file, "Images/Products");
                product.ImageUrl = imageUrl;

                await _unitOfWork.Repository<Product>().AddAsync(product);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                if(imageUrl != null)
                {
                    _fileService.DeleteFile(imageUrl);
                }
                return false;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = _unitOfWork.Repository<Product>().GetAsQuerable().FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return false;
            }
            var oldImageUrl = product.ImageUrl;
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _unitOfWork.Repository<Product>().Delete(product);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
               
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
                return false;
            }
            if (!_fileService.DeleteFile(oldImageUrl))
            {
                _logger.LogWarning(
                    "Failed to delete image for product {ProductId}", id);
            }
            return true;       
        }

        public async Task<List<Product>> GetAllProductsAsync(bool onlyActive = false, bool includeCategories = false)
        {
            IQueryable<Product> products =  _unitOfWork.Repository<Product>().GetAsQuerable().AsNoTracking();
            if(includeCategories)
            {
                products = products.Include(p => p.Category);
            }
            if (onlyActive)
            {
                products = products.Where(p => p.IsActive);
            }
            return await products.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id, bool includeCategories = false)
        {
            IQueryable<Product> products = _unitOfWork.Repository<Product>().GetAsQuerable();
            if (includeCategories)
            {
                products = products.Include(p => p.Category);
            }
            return await products.FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<bool> UpdateProductAsync(Product product, IFormFile? file)
        {
            string? newImageUrl = null;
            try
            {
                if (product == null)
                {
                    return false;
                }
                var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id);
                if (existingProduct == null)
                {
                    return false;
                }
                await _unitOfWork.BeginTransactionAsync();

                var oldUrl = existingProduct.ImageUrl;
                if (file != null && file.Length > 0)
                {
                    newImageUrl = await _fileService.UploadFileAsync(file, "Images/Products");
                    existingProduct.ImageUrl = newImageUrl;
                }

                existingProduct.Code = product.Code;
                existingProduct.Name = product.Name;
                existingProduct.SalesPrice = product.SalesPrice;
                existingProduct.CostPrice = product.CostPrice;
                existingProduct.IsActive = product.IsActive;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;

                await _unitOfWork.SaveChangesAsync();

                await _unitOfWork.CommitTransactionAsync();

                if (oldUrl != null && newImageUrl != null)
                {
                    var isImageDeleted = _fileService.DeleteFile(oldUrl);
                    if (!isImageDeleted)
                    {
                        _logger.LogWarning(
                                            "Failed to delete old image for product {ProductId}",
                                            product.Id);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
                if(newImageUrl != null)
                {
                    _fileService.DeleteFile(newImageUrl);
                }
                return false;
            }
        }
        public async Task<bool> IsProductCodeExistsAsync(string code, int? productId = null)
        {
            var query = _unitOfWork.Repository<Product>().GetAsQuerable();
            var isExists = false;
            if (productId != null)
            {
                isExists = query.Any(p => p.Code == code && p.Id != productId);
            }
            else
            {
                isExists = query.Any(p => p.Code == code);
            }
            return isExists;
        }
    }
}
