using ELibrary.Application.Common.Interfaces;
using ELibrary.Domain.Entities;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork
    ) : IRequestHandler<CreateCategoryCommand, ErrorOr<EntityId>> 
{
    public async Task<ErrorOr<EntityId>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var categoryResult = Category.Create(command.Name);

        if (categoryResult.IsError)
            return categoryResult.Errors;

        var category = categoryResult.Value;

        categoryRepository.Add(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}