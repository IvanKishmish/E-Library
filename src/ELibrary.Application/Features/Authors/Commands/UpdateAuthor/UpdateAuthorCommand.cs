using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.UpdateAuthor;

public sealed record UpdateAuthorCommand(
    EntityId Id,
    string FirstName,
    string LastName,
    string Biography) : IRequest<ErrorOr<Updated>>, IInvalidateCacheCommand
{
    public string[] CacheKeysToInvalidate => [ "all-authors", $"author-{Id}" ];
}