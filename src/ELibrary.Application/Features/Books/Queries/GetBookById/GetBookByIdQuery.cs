using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Books.Queries.GetBookById;

public sealed record GetBookByIdQuery(EntityId Id) : IRequest<ErrorOr<BookResponseDto>>;