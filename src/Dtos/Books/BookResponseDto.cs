namespace E_Library.Dtos.Books;

public sealed record BookResponseDto(
    EntityId Id,
    string Title,
    string Description,
    decimal Price,
    EntityId CategoryId,
    string CategoryName, 
    EntityId AuthorId
    );