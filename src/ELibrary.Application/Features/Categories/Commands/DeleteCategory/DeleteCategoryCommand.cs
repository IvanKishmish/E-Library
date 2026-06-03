using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(EntityId Id) : IRequest<ErrorOr<Deleted>>;