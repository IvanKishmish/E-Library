using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    EntityId Id,
    string Name) : IRequest<ErrorOr<Updated>>, IInvalidateCacheCommand
{
    // Якщо категорія оновилася, треба знести і загальний список, і кеш цієї конкретної категорії за її ID
    public string[] CacheKeysToInvalidate => [ "all-categories", $"category-{Id}" ];
}