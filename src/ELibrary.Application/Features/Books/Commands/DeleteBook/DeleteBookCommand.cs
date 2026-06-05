using ELibrary.Application.Common.Interfaces;

namespace ELibrary.Application.Features.Books.Commands.DeleteBook;
using MediatR;
using ErrorOr;

public sealed record DeleteBookCommand(EntityId Id) : IRequest<ErrorOr<Deleted>>, IInvalidateCacheCommand
{
    public string[] CacheKeysToInvalidate => [ "all-book", $"book-{Id}" ];
}