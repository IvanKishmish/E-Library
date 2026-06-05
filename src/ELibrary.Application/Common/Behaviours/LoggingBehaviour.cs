using MediatR;
using Microsoft.Extensions.Logging;

namespace ELibrary.Application.Common.Behaviours;

// Цей клас перехоплює будь-який запит (TRequest), який летить через MediatR
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Логуємо початок виконання команди/запиту
        logger.LogInformation("Запуск MediatR запиту: {RequestName}", requestName);

        try
        {
            // Передаємо управління далі по конвеєру (до наступного Behavior або до самого Хендлера)
            var response = await next(cancellationToken);

            logger.LogInformation("Запит {RequestName} успішно оброблений.", requestName);
            
            return response;
        }
        catch (Exception ex)
        {
            // Якщо всередині хендлера сталася непередбачувана помилка — логуємо її глобально
            logger.LogError(ex, "Помилка під час виконання запиту {RequestName}", requestName);
            throw;
        }
    }
}