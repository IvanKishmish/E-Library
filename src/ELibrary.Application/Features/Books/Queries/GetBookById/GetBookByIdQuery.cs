using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Queries.GetBookById;

public sealed record GetBookByIdQuery(EntityId Id) : IRequest<ErrorOr<BookResponseDto>>, ICachableQuery
{
    public string CacheKey => $"book-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}