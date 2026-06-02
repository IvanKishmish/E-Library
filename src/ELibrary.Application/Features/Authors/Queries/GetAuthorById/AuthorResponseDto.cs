namespace ELibrary.Application.Features.Authors.Queries.GetAuthorById;

public sealed record AuthorResponseDto(
    EntityId Id,
    string FirstName,
    string LastName,
    string Biography,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);