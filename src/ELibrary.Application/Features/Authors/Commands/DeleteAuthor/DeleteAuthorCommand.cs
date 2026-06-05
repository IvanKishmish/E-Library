using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.DeleteAuthor;

public sealed record DeleteAuthorCommand(EntityId Id) : IRequest<ErrorOr<Deleted>>, IInvalidateCacheCommand
{
    public string[] CacheKeysToInvalidate => [ "all-authors", $"author-{Id}" ];
}