using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Authors.Queries.GetAllAuthors;

public sealed record GetAllAuthorsQuery : IRequest<ErrorOr<IReadOnlyList<AuthorSummaryDto>>>;