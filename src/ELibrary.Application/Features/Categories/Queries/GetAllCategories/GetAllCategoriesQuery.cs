using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery : IRequest<ErrorOr<IReadOnlyList<CategorySummaryDto>>>, ICachableQuery
{
    public string CacheKey => "all-categories";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}