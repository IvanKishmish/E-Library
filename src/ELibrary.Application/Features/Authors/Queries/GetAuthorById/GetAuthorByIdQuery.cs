using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Authors.Queries.GetAuthorById;

public sealed record GetAuthorByIdQuery(EntityId Id) : IRequest<ErrorOr<AuthorResponseDto>>;