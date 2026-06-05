using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(ICategoryRepository categoryRepository)
: IRequestHandler<GetCategoryByIdQuery, ErrorOr<CategoryResponseDto>>
{
    public async Task<ErrorOr<CategoryResponseDto>> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(query.Id, cancellationToken);
        
        if(category is null)
            return Error.NotFound("Category.NotFound", $"Category with id '{query.Id}' was not found.");

        return new CategoryResponseDto(
            category.Id,
            category.Name,
            category.CreatedAt,
            category.UpdatedAt);
    }
}