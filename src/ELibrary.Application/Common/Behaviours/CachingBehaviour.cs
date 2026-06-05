using System.Text.Json;
using ELibrary.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Common.Behaviours;

public sealed class CachingBehavior<TRequest, TResponse>(
    IDistributedCache cache,
    ILogger<CachingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachableQuery
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var cacheKey = request.CacheKey;

        // Спробуємо дістати дані з кешу
        var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            logger.LogInformation("--- [CACHE HIT] Дані для запиту {RequestName} взято з кешу по ключу: {CacheKey}", requestName, cacheKey);
            
            var result = JsonSerializer.Deserialize<TResponse>(cachedData);
            return result!;
        }

        // Якщо в кеші порожньо — йдемо далі по конвеєру (в хендлер до бази даних)
        logger.LogInformation("--- [CACHE MISS] У кеші порожньо для {RequestName}. Йдемо в базу...", requestName);
        var response = await next(cancellationToken);

        // Перевіряємо результат: якщо це ErrorOr і всередині є помилка — не кешуємо її
        if (response is IErrorOr errorOrResult && errorOrResult.IsError)
        {
            return response; 
        }

        // Записуємо успішний результат у кеш
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = request.Expiration ?? TimeSpan.FromMinutes(5)
        };

        var serializedData = JsonSerializer.Serialize(response);
        await cache.SetStringAsync(cacheKey, serializedData, options, cancellationToken);

        return response;
    }
}