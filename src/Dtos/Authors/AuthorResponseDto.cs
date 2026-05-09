using E_Library.Database.Entities;

namespace E_Library.Dtos.Authors;

public record AuthorResponseDto(
    Guid Id,
    string FullName,
    string Biography,
    List<Book> Books
    );