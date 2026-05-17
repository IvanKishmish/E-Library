using E_Library.Dtos.Authors;
using E_Library.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace E_Library.Controllers;


/// <summary>
/// в result.Match якщо в ErrorOr лежать дані - виконується перша функція(Успіх),
/// якщо там список помилок - виконується друга функція(Провал)
/// </summary>
/// <param name="service"></param>
[ApiController]
[Route("api/[controller]s")]
public class AuthorsController(IAuthorService service) : ApiController 
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
    public async Task<IActionResult> Create(CreateAuthorRequestDto dto)
    {
        var result = await service.CreateAsync(dto);
        return result.Match(
            author => CreatedAtAction(nameof(GetById), new { id = author.Id }, author),
            Problem
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(EntityId id, UpdateAuthorRequestDto dto)
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