using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Queries.GetAuthorById;

public sealed class GetAuthorByIdQueryHandler(IAuthorRepository authorRepository)
    : IRequestHandler<GetAuthorByIdQuery, ErrorOr<AuthorResponseDto>>
{
    public async Task<ErrorOr<AuthorResponseDto>> Handle(
        GetAuthorByIdQuery query,
        CancellationToken cancellationToken)
    {
        var author = await authorRepository.GetByIdAsync(query.Id, cancellationToken);

        if (author is null)
            return Error.NotFound("Author.NotFound", $"Author with id '{query.Id}' was not found.");

        return new AuthorResponseDto(
            author.Id,
            author.FirstName,
            author.LastName,
            author.Biography,
            author.CreatedAt,
            author.UpdatedAt);
    }
}