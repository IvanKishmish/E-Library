using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Queries.GetAllBooks;

public sealed record GetAllBooksQuery : IRequest<ErrorOr<IReadOnlyList<BookSummaryDto>>>;