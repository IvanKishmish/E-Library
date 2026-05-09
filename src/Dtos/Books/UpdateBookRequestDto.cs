using E_Library.Database.Entities;

namespace E_Library.Dtos.Books;

public record UpdateBookRequestDto(
    string Title,
    string Description,
    decimal Price,
    Author? Author,
    Guid AuthorId,
    Guid CategoryId
    );