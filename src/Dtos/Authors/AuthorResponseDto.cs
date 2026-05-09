using E_Library.Database.Entities;

namespace E_Library.Dtos.Authors;

public sealed record AuthorResponseDto(
    Guid Id,
    string FullName,
    string Biography,
    List<Book> Books
    );