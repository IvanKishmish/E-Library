using E_Library.Dtos.Books;
using FluentValidation;

namespace E_Library.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookRequestDto>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Назва книги обов'язкова")
            .MaximumLength(200).WithMessage("Назва не може бути довшою за 200 символів");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Опис книги обов'язковий")
            .MinimumLength(10).WithMessage("Опис має бути хоча б на 10 символів");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Ціна не може бути від'ємною")
            .LessThan(1000000).WithMessage("Ціна занадто висока");

        // Перевіряємо, чи ID не порожні
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Потрібно вказати категорію");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Потрібно вказати автора");
    }
}