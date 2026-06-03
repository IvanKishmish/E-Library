using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Categories.Queries.GetAllCategories;

public sealed class GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetAllCategoriesQuery, ErrorOr<IReadOnlyList<CategorySummaryDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<CategorySummaryDto>>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);

        var dtos = categories
            .Select(c => new CategorySummaryDto(c.Id, c.Name))
            .ToList();

        return dtos;
    }
}