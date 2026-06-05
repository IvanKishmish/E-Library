using ELibrary.Application.Features.Books.Commands.CreateBook;
using ELibrary.Application.Features.Books.Commands.DeleteBook;
using ELibrary.Application.Features.Books.Commands.UpdateBook;
using ELibrary.Application.Features.Books.Queries.GetAllBooks;
using ELibrary.Application.Features.Books.Queries.GetBookById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApi.Controllers;

[Route("api/[controller]s")]
public sealed class BookController(ISender mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllBooksQuery();
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.Match(
            books => Ok(books),
            errors => Problem(errors)
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(EntityId id, CancellationToken cancellationToken)
    {
        var query = new GetBookByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.Match(
            book => Ok(book),
            errors => Problem(errors)
        );
    }

    
    /// <remarks>
    /// Навіщо тут два рази id? 
    /// Перший `new { id = bookId }` підставляється у шаблон маршруту "GetById" для генерації HTTP-заголовка Location.
    /// Другий `new { id = bookId }` формує JSON-тіло відповіді { "id": "..." } для клієнта.
    /// </remarks>
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.Match(
            bookId => CreatedAtAction(nameof(GetById),
                new { id = bookId }, // 1. Будує правильний URL для заголовка Location (наприклад, /api/Books/018f3b...)
                new { id = bookId }), // 2. Повертає клієнту JSON-об'єкт із полем id у тілі відповіді
            errors => Problem(errors)
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(EntityId id, UpdateBookCommand requestCommand, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(requestCommand with {Id = id}, cancellationToken);
        
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(EntityId id, CancellationToken cancellationToken)
    {
        var command = new DeleteBookCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }
}