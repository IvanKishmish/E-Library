using MediatR;
using ErrorOr;
using ELibrary.Application.Common.Interfaces;

namespace ELibrary.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<UpdateCategoryCommand, ErrorOr<Updated>>
{
    public async Task<ErrorOr<Updated>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(command.Id, cancellationToken);
        
        if(category is null)
            return Error.NotFound("Category.NotFound", $"Category with id '{command.Id}' was not found.");

        var updateResult = category.UpdateName(command.Name);

        if (updateResult.IsError)
            return updateResult.Errors;
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}