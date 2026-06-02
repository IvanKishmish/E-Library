using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.DeleteAuthor;

public sealed class DeleteAuthorCommandHandler(
    IAuthorRepository authorRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteAuthorCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(
        DeleteAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = await authorRepository.GetByIdAsync(command.Id, cancellationToken);

        if (author is null)
            return Error.NotFound("Author.NotFound", $"Author with id '{command.Id}' was not found.");

        authorRepository.Remove(author);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}