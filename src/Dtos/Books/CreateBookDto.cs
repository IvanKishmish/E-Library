namespace E_Library.Dtos.Books;

public sealed record CreateBookDto(
    string Title,
    string Description,
    decimal Price,
    Guid AuthorId,
    Guid CategoryId
    );