using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Commands.CreateBook;

public sealed class CreateBookCommandHandler(
    IBookRepository bookRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateBookCommand, ErrorOr<EntityId>>
{
    public async Task<ErrorOr<EntityId>> Handle(CreateBookCommand command, CancellationToken cancellationToken)
    {
        var bookResult = Book
            .Create(command.Title, command.Description, command.Price, command.CategoryId, command.AuthorId);

        if (bookResult.IsError)
            return bookResult.Errors;
        
        var book = bookResult.Value;
        
        bookRepository.Add(book);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return book.Id;
    }
}