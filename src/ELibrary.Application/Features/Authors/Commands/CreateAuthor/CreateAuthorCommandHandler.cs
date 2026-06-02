using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.CreateAuthor;

public sealed class CreateAuthorCommandHandler(
    IAuthorRepository authorRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAuthorCommand, ErrorOr<EntityId>>
{
    public async Task<ErrorOr<EntityId>> Handle(
        CreateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var authorResult = Author.Create(
            command.FirstName,
            command.LastName,
            command.Biography);

        if (authorResult.IsError)
            return authorResult.Errors;

        var author = authorResult.Value;

        authorRepository.Add(author);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return author.Id;
    }
}