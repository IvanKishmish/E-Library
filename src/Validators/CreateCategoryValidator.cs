using E_Library.Dtos.Categories;
using FluentValidation;

namespace E_Library.Validators;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryRequestDto>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Назва категорії обов'язкова")
            .MinimumLength(2).WithMessage("Назва занадто коротка")
            .MaximumLength(50).WithMessage("Назва не може бути довшою за 50 символів");
    }
}