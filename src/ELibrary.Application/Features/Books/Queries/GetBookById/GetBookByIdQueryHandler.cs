using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Queries.GetBookById;

public sealed class GetBookByIdQueryHandler(IBookRepository bookRepository)
: IRequestHandler<GetBookByIdQuery, ErrorOr<BookResponseDto>>
{
    public async Task<ErrorOr<BookResponseDto>> Handle(GetBookByIdQuery query, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(query.Id, cancellationToken);
        
        if(book is null)
            return Error.NotFound("Book.NotFound", $"Book with id '{query.Id}' was not found.");

        return new BookResponseDto(
            book.Id,
            book.Title,
            book.Description,
            book.Price,
            book.AuthorId,
            book.Author.FirstName + " " + book.Author.LastName,
            book.CategoryId,
            book.Category.Name,
            book.CreatedAt,
            book.UpdatedAt);
    }
}