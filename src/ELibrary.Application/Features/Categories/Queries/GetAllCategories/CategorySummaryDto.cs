namespace ELibrary.Application.Features.Categories.Queries.GetAllCategories;

public sealed record CategorySummaryDto(
    EntityId Id,
    string Name);