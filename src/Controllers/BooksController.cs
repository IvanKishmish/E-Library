using E_Library.Dtos.Books;
using E_Library.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace E_Library.Controllers;

[ApiController]
[Route("api/[controller]s")]
public class BooksController(IBookService bookService) : ApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll() 
    {
        var result = await bookService.GetAllAsync();
        return result.Match(Ok, Problem);
    }  
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(EntityId id)
    {
        var result = await bookService.GetByIdAsync(id);
        return result.Match(Ok, Problem);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookRequestDto dto)
    {
        var result = await bookService.CreateAsync(dto);
        return result.Match(
            book => CreatedAtAction(nameof(GetById), new { id = book.Id }, book),
            Problem
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(EntityId id, UpdateBookRequestDto dto)
    {
        var result = await bookService.UpdateAsync(id, dto);
        return result.Match(_ => NoContent(), Problem);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(EntityId id)
    {
        var result = await bookService.DeleteAsync(id);
        return result.Match(_ => NoContent(), Problem);
    }
    
    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered([FromQuery] BookFilterParameters filter)
    {
        var result = await bookService.GetFilteredAsync(filter);
    
        return result.Match(
            books => Ok(books),
            errors => Problem(errors)
        );
    }
}