using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Authors.Queries.GetAllAuthors;

public sealed record GetAllAuthorsQuery : IRequest<ErrorOr<IReadOnlyList<AuthorSummaryDto>>>, ICachableQuery
{
    public string CacheKey => "all-authors";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(30);
}