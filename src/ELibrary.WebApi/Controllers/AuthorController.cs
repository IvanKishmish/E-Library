using ELibrary.Application.Features.Authors.Commands.CreateAuthor;
using ELibrary.Application.Features.Authors.Commands.DeleteAuthor;
using ELibrary.Application.Features.Authors.Commands.UpdateAuthor;
using ELibrary.Application.Features.Authors.Queries.GetAllAuthors;
using ELibrary.Application.Features.Authors.Queries.GetAuthorById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.WebApi.Controllers;

[Route("api/[controller]s")]
public sealed class AuthorController(ISender mediator) : ApiController(mediator)
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllAuthorsQuery();
        var result = await Mediator.Send(query, cancellationToken);
        
        return result.Match(
            authors => Ok(authors),
            errors => Problem(errors)
            );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(EntityId id, CancellationToken cancellationToken)
    {
        var query = new GetAuthorByIdQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        return result.Match(
            author => Ok(author),
            errors => Problem(errors)
        );
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateAuthorCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        return result.Match(
            authorId => CreatedAtAction(nameof(GetById),
                new { id = authorId },
                new { id = authorId }),
            errors => Problem(errors)
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(EntityId id, UpdateAuthorCommand requestCommand, CancellationToken cancellationToken)
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
        var command = new DeleteAuthorCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        
        return result.Match(
            _ => NoContent(),
            errors => Problem(errors)
        );
    }
}