namespace ELibrary.Application.Features.Books.Queries.GetAllBooks;

public sealed record BookSummaryDto(
    EntityId Id,
    string Title,
    decimal Price,
    string AuthorFullName,
    string CategoryName);