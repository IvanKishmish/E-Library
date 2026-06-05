using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name) : IRequest<ErrorOr<EntityId>>, IInvalidateCacheCommand
{
    // Вказуємо, що після створення категорії треба знести загальний список "all-categories"
    public string[] CacheKeysToInvalidate => ["all-categories"];
}