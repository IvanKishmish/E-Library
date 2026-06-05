namespace ELibrary.Application.Features.Books.Queries.GetBookById;

public sealed record BookResponseDto(
    EntityId Id,
    string Title,
    string Description,
    decimal Price,
    EntityId AuthorId,
    string AuthorFullName,
    EntityId CategoryId,
    string CategoryName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);