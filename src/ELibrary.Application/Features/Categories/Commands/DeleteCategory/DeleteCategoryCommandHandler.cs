using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<DeleteCategoryCommand, ErrorOr<Deleted>>
{
    public async Task<ErrorOr<Deleted>> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(command.Id, cancellationToken);
        
        if(category is null)
            return Error.NotFound("Category.NotFound", $"Category with id '{command.Id}' was not found.");
        
        categoryRepository.Remove(category);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}