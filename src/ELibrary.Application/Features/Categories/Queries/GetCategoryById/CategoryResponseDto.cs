namespace ELibrary.Application.Features.Categories.Queries.GetCategoryById;

public sealed record CategoryResponseDto(
    EntityId Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);