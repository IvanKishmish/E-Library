using E_Library.Dtos.Categories;
using E_Library.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace E_Library.Controllers;

[ApiController]
[Route("api/[controller]s")]
public class CategoriesController(ICategoryService service) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await service.GetAllAsync();
        return result.Match(Ok, Problem);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(EntityId id)
    {
        var result = await service.GetByIdAsync(id);
        return result.Match(Ok, Problem);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryRequestDto dto)
    {
        var result = await service.CreateAsync(dto);
        
        return result.Match(
            category => CreatedAtAction(nameof(GetById), new { id = category.Id }, category),
            Problem
        );
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(EntityId id, UpdateCategoryRequestDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(EntityId id)
    {
        var result = await service.DeleteAsync(id);
        return result.Match(_ => NoContent(), Problem);
    }
}