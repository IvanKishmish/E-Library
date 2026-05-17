namespace E_Library.Dtos.Books;

public sealed record UpdateBookRequestDto(
    string Title,
    string Description,
    decimal Price,
    EntityId CategoryId
    );