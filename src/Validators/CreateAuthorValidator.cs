using E_Library.Dtos.Authors;
using FluentValidation;

namespace E_Library.Validators;

public class CreateAuthorValidator : AbstractValidator<CreateAuthorRequestDto>
{
    public CreateAuthorValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Ім'я автора не може бути порожнім")
            .MinimumLength(3).WithMessage("Ім'я занадто коротке")
            .MaximumLength(100).WithMessage("Ім'я не може перевищувати 100 символів")
            .Must(name => name.Trim().Split(' ').Length >= 2)
            .WithMessage("Вкажіть повне ім'я (мінімум ім'я та прізвище)");

        RuleFor(x => x.Biography)
            .MaximumLength(1000).WithMessage("Біографія занадто довга (макс. 1000 символів)");
    }
}