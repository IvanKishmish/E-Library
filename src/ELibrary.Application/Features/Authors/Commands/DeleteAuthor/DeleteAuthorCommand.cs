using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Commands.DeleteAuthor;

public sealed record DeleteAuthorCommand(EntityId Id) : IRequest<ErrorOr<Deleted>>;