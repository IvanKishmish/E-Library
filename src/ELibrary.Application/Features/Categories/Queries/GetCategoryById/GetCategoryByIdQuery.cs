using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(EntityId Id) : IRequest<ErrorOr<CategoryResponseDto>>, ICachableQuery
{
    public string CacheKey => $"category-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}