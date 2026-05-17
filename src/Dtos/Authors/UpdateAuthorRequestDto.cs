using E_Library.Database.Entities;

namespace E_Library.Dtos.Authors;

public sealed record UpdateAuthorRequestDto(
    string FullName,
    string Biography,
    List<Book> Books);