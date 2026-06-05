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

        // 1. Спробуємо дістати дані з кешу
        var cachedData = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            logger.LogInformation("--- [CACHE HIT] Дані для запиту {RequestName} взято з кешу по ключу: {CacheKey}", requestName, cacheKey);
    
            // Дістаємо тип чистих даних, який лежить всередині ErrorOr (наприклад, List<CategorySummaryDto>)
            var valueType = typeof(TResponse).GetGenericArguments()[0];
            var underlyingData = JsonSerializer.Deserialize(cachedData, valueType);

            //Знаходимо оператор неявного приведення типів (implicit operator)
            var implicitOperator = typeof(TResponse)
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Implicit" && m.GetParameters()[0].ParameterType == valueType);

            if (implicitOperator is null)
            {
                throw new InvalidOperationException($"Не вдалося знайти магічний op_Implicit для типу {typeof(TResponse).Name}");
            }

            // Викликаємо цей оператор і передаємо йому десеріалізовані з кешу дані
            var errorOrResult = implicitOperator.Invoke(null, [underlyingData]);

            return (TResponse)errorOrResult!;
        }

        // 2. Якщо в кеші порожньо — йдемо в базу
        logger.LogInformation("--- [CACHE MISS] У кеші порожньо для {RequestName}. Йдемо в базу...", requestName);
        var response = await next(cancellationToken);

        // Перевіряємо результат: якщо є помилка — не кешуємо
        if (response is IErrorOr errorOrResultCheck && errorOrResultCheck.IsError)
        {
            return response; 
        }

        // 3. Записуємо ТІЛЬКИ чисті дані у кеш (дістаємо властивість .Value через рефлексію)
        var valueProperty = response!.GetType().GetProperty("Value");
        var rawDataToCache = valueProperty?.GetValue(response);

        if (rawDataToCache is not null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = request.Expiration ?? TimeSpan.FromMinutes(5)
            };

            var serializedData = JsonSerializer.Serialize(rawDataToCache);
            await cache.SetStringAsync(cacheKey, serializedData, options, cancellationToken);
        }

        return response;
    }
}