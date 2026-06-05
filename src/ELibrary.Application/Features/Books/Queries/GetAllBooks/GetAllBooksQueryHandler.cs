using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Queries.GetAllBooks;

public sealed class GetAllBooksQueryHandler(
    IBookRepository bookRepository) : IRequestHandler<GetAllBooksQuery, ErrorOr<IReadOnlyList<BookSummaryDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<BookSummaryDto>>> Handle(GetAllBooksQuery query, CancellationToken cancellationToken)
    {
        var books = await bookRepository.GetAllAsync(cancellationToken);
        
        var dtos = books.Select(b => 
            new BookSummaryDto(b.Id, b.Title, b.Price, b.Author.FirstName +  " " + b.Author.LastName, b.Category.Name))
            .ToList();

        return dtos;
    }
}