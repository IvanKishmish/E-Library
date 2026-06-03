using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery : IRequest<ErrorOr<IReadOnlyList<CategorySummaryDto>>>;