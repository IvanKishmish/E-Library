using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.UpdateAuthor;

public sealed class UpdateAuthorCommandHandler(
    IAuthorRepository authorRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateAuthorCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(
        UpdateAuthorCommand command,
        CancellationToken cancellationToken)
    {
        var author = await authorRepository.GetByIdAsync(command.Id, cancellationToken);

        if (author is null)
            return Error.NotFound("Author.NotFound", $"Author with id '{command.Id}' was not found.");

        var updateResult = author.UpdateInfo(
            command.FirstName,
            command.LastName,
            command.Biography);

        if (updateResult.IsError)
            return updateResult.Errors;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}