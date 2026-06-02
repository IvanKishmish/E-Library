namespace ELibrary.Application.Features.Authors.Queries.GetAllAuthors;

public sealed record AuthorSummaryDto(
    EntityId Id,
    string FirstName,
    string LastName);