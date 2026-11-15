using AutoMapper;
using ERP.Application.Dto.Categories;
using ERP.Application.Interfaces;
using ERP.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IMapper _mapper;

    public CategoriesController(ICategoryService categoryService, IMapper mapper)
    {
        _categoryService = categoryService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();
        return Ok(_mapper.Map<List<CategoryDto>>(categories));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category is null) return NotFound();

        return Ok(_mapper.Map<CategoryDto>(category));
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(SaveCategoryDto dto)
    {
        if (await _categoryService.IsCategoryNameExistsAsync(dto.Name))
            return Conflict(new { message = "Category name already exists" });

        var category = _mapper.Map<Category>(dto);
        var created = await _categoryService.CreateCategoryAsync(category);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, _mapper.Map<CategoryDto>(created));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, SaveCategoryDto dto)
    {
        if (await _categoryService.GetCategoryByIdAsync(id) is null)
            return NotFound();

        if (await _categoryService.IsCategoryNameExistsAsync(dto.Name, id))
            return Conflict(new { message = "Category name already exists" });

        var category = _mapper.Map<Category>(dto);
        category.Id = id;

        var updated = await _categoryService.UpdateCategoryAsync(category);
        return Ok(_mapper.Map<CategoryDto>(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            return deleted ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "Cannot delete a category that has products" });
        }
    }
}