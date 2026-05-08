using E_Library.Database.Entities;

namespace E_Library.Dtos.Books;

public sealed record BookResponseDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    Author Author,
    Category Category
    );