using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Queries.GetAuthorById;

public sealed record GetAuthorByIdQuery(EntityId Id) : IRequest<ErrorOr<AuthorResponseDto>>, ICachableQuery
{
    // Формуємо унікальний ключ для кешу, наприклад: "author-018f3b..."
    public string CacheKey => $"author-{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
}