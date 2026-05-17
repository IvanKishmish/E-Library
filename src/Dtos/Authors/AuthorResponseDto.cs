namespace E_Library.Dtos.Authors;

public sealed record AuthorResponseDto(
    EntityId Id,
    string FullName,
    string Biography,
    IEnumerable<string> BookTitles
    );