namespace E_Library.Dtos.Authors;

public sealed record CreateAuthorRequestDto(
    string FullName,
    string Biography
    );