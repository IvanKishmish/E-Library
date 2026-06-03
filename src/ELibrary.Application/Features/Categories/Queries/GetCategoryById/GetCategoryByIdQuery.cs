using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(EntityId Id) : IRequest<ErrorOr<CategoryResponseDto>>;