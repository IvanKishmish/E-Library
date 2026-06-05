using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Books.Commands.UpdateBook;

public sealed record UpdateBookCommand(
    EntityId Id,
    string Title,
    string Description,
    decimal Price,
    EntityId CategoryId,
    EntityId AuthorId) : IRequest<ErrorOr<Updated>>, IInvalidateCacheCommand
{
    public string[] CacheKeysToInvalidate => [ "all-books", $"book-{Id}" ];
}