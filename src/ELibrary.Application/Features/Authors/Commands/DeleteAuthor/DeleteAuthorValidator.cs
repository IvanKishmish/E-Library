using FluentValidation;

namespace ELibrary.Application.Features.Authors.Commands.DeleteAuthor;

public sealed class DeleteAuthorValidator : AbstractValidator<DeleteAuthorCommand>
{
    public DeleteAuthorValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Author id is required.");
    }
}