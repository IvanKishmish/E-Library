using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MediatR;

namespace ELibrary.Application.Common.Behaviours;

public sealed class CacheInvalidationBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
where TRequest : IInvalidateCacheCommand // Перехоплюємо тільки ті команди, які мають цей маркер
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        // Спочатку пускаємо команду далі по конвеєру, щоб вона виконалася в базі даних (Unit of Work збереже зміни)
        var response = await next(cancellationToken);
        
        // Перевіряємо результат: якщо команда завершилася помилкою (наприклад, валідація не пройшла), 
        // то кеш чистити не треба, адже в базі нічого не змінилося.
        if (response is IErrorOr errorOrResult && errorOrResult.IsError)
        {
            return response;
        }
        
        // Якщо команда успішна — зносимо застарілий кеш
        if (request.CacheKeysToInvalidate.Length > 0)
        {
            logger.LogInformation("--- [CACHE INVALIDATION] Команда {RequestName} успішна. Очищення пов'язаного кешу...", requestName);

            foreach (var key in request.CacheKeysToInvalidate)
            {
                await cache.RemoveAsync(key, cancellationToken);
                logger.LogInformation("--- [CACHE REMOVED] Ключ очищено: {CacheKey}", key);
            }
        }

        return response;
    }
}