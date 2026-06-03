using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    EntityId Id,
    string Name) : IRequest<ErrorOr<Updated>>;