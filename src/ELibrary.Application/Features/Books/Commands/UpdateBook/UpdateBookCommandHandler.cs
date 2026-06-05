using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Commands.UpdateBook;

public sealed class UpdateBookCommandHandler(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<UpdateBookCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateBookCommand command, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(command.Id, cancellationToken);

        if (book is null)
            return Error.NotFound("Book.NotFound", $"Book with id '{command.Id}' was not found.");
        
        var updateResult = book
            .UpdateInfo(command.Title, command.Description, command.Price, command.CategoryId, command.AuthorId);

        if (updateResult.IsError)
            return updateResult.Errors;
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}