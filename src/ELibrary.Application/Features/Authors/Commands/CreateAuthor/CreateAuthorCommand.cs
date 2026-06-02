using ErrorOr;
using MediatR;
 
namespace ELibrary.Application.Features.Authors.Commands.CreateAuthor;
 
public sealed record CreateAuthorCommand(
    string FirstName,
    string LastName,
    string Biography) : IRequest<ErrorOr<EntityId>>;