using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Books.Commands.DeleteBook;

public sealed class DeleteBookCommandHandler(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteBookCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteBookCommand command, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(command.Id, cancellationToken);
        
        if (book is null)
            return Error.NotFound("Book.NotFound", $"Book with id '{command.Id}' was not found.");
        
        bookRepository.Remove(book);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Deleted;
    }
}