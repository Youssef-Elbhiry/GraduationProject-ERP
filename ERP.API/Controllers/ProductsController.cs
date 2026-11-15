using AutoMapper;
using ERP.Application.Dto.Products;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public ProductsController(IProductService productService, ICategoryService categoryService, IMapper mapper)
    {
        _productService = productService;
        _categoryService = categoryService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetAll(bool onlyActive = false)
    {
        var products = await _productService.GetAllProductsAsync(onlyActive, includeCategories: true);
        return Ok(_mapper.Map<List<ProductDto>>(products));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id, includeCategories: true);
        if (product is null) return NotFound();

        return Ok(_mapper.Map<ProductDto>(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromForm] SaveProductDto dto, IFormFile image)
    {
        if (await _categoryService.GetCategoryByIdAsync(dto.CategoryId) is null)
            return BadRequest(new { message = "Category does not exist" });

        if (await _productService.IsProductCodeExistsAsync(dto.Code))
            return Conflict(new { message = "Product code already exists" });

        var product = _mapper.Map<Product>(dto);

        var success = await _productService.CreateProductAsync(product, image);
        if (!success)
            return BadRequest(new { message = "Failed to create product (image must be jpg/png/webp, max 2MB)" });

        var created = await _productService.GetProductByIdAsync(product.Id, includeCategories: true);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, _mapper.Map<ProductDto>(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromForm] SaveProductDto dto, IFormFile? image)
    {
        if (await _productService.GetProductByIdAsync(id) is null)
            return NotFound();

        if (await _categoryService.GetCategoryByIdAsync(dto.CategoryId) is null)
            return BadRequest(new { message = "Category does not exist" });

        if (await _productService.IsProductCodeExistsAsync(dto.Code, id))
            return Conflict(new { message = "Product code already exists" });

        var product = _mapper.Map<Product>(dto);
        product.Id = id;

        var success = await _productService.UpdateProductAsync(product, image);
        if (!success)
            return BadRequest(new { message = "Failed to update product" });

        var updated = await _productService.GetProductByIdAsync(id, includeCategories: true);
        return Ok(_mapper.Map<ProductDto>(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _productService.DeleteProductAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}