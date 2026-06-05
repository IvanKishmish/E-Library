using FluentValidation;

namespace ELibrary.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category id is required");
    }
}