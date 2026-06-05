using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Queries.GetAllBooks;

public sealed record GetAllBooksQuery : IRequest<ErrorOr<IReadOnlyList<BookSummaryDto>>>, ICachableQuery
{
    public string CacheKey => "all-books";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}