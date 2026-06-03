using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name
    ) : IRequest<ErrorOr<EntityId>>;