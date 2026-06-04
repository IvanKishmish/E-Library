using ErrorOr;
using MediatR;

namespace ELibrary.Application.Features.Books.Commands.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string Description,
    decimal Price,
    EntityId CategoryId,
    EntityId AuthorId) : IRequest<ErrorOr<EntityId>>;