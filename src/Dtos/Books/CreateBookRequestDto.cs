namespace E_Library.Dtos.Books;

public sealed record CreateBookRequestDto(
    string Title,
    string Description,
    decimal Price,
    EntityId CategoryId,
    EntityId AuthorId
    );