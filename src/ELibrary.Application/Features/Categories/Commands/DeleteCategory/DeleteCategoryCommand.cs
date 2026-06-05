using ELibrary.Application.Common.Interfaces;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(EntityId Id) : IRequest<ErrorOr<Deleted>>, IInvalidateCacheCommand
{
    // Очищаємо загальний список та кеш конкретної категорії, яку щойно видалили
    public string[] CacheKeysToInvalidate => [ "all-categories", $"category-{Id}" ];
}