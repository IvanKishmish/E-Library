using FluentValidation;

namespace ELibrary.Application.Features.Books.Commands.UpdateBook;

public sealed class UpdateBookValidator : AbstractValidator<UpdateBookCommand>
{
    public UpdateBookValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Book title is required.")
            .MaximumLength(150).WithMessage("Book title must not exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Book description is required.")
            .MaximumLength(1000).WithMessage("Book description must not exceed 1000 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be less than zero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required.");
    }
}