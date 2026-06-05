using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.CreateAuthor;

/// <summary>
/// Create має повертати EntityId, тому що цей ID створюється на бекенді, і фронтенд без нього не
/// зможе працювати далі (не зможе відкрити сторінку автора або додати його в якийсь список на клієнті)
/// </summary>
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