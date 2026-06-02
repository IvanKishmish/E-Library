using FluentValidation;
using MediatR;
using ErrorOr;

namespace ELibrary.Application.Common.Behaviours;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // 1. Якщо для цієї команди немає жодного валідатора — просто йдемо далі
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        // 2. Створюємо контекст валідації для FluentValidation
        var context = new ValidationContext<TRequest>(request);

        // 3. Запускаємо всі валідатори, які знайшли для цієї команди
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // 4. Збираємо всі помилки докупи в один список
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // 5. Якщо помилок немає — супер, пропускаємо команду в хендлер
        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        // 6. ПРОСТИЙ ВАРІАНТ: Створюємо список чистих помилок ErrorOr
        var errors = new List<Error>();
        
        foreach (var failure in failures)
        {
            // Створюємо стандартну валідаційну помилку з ErrorOr
            var error = Error.Validation(
                code: failure.PropertyName, 
                description: failure.ErrorMessage);
                
            errors.Add(error);
        }

        // 7. Повертаємо список помилок назад на контролер, не пускаючи код у хендлер
        return (TResponse)(dynamic)errors;
    }
}