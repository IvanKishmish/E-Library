using FluentValidation;

namespace ELibrary.Application.Features.Books.Commands.DeleteBook;

public sealed class DeleteBookValidator : AbstractValidator<DeleteBookCommand>
{
    public DeleteBookValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Book ID is required.");
    }
}