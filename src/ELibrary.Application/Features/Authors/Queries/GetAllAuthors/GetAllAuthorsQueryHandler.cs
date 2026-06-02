using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Queries.GetAllAuthors;

public sealed class GetAllAuthorsQueryHandler(IAuthorRepository authorRepository)
    : IRequestHandler<GetAllAuthorsQuery, ErrorOr<IReadOnlyList<AuthorSummaryDto>>>
{
    public async Task<ErrorOr<IReadOnlyList<AuthorSummaryDto>>> Handle(
        GetAllAuthorsQuery query,
        CancellationToken cancellationToken)
    {
        var authors = await authorRepository.GetAllAsync(cancellationToken);

        var dtos = authors
            .Select(a => new AuthorSummaryDto(a.Id, a.FirstName, a.LastName))
            .ToList();

        return dtos;
    }
}