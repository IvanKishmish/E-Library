using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ErrorOr;

namespace ELibrary.WebApi.Controllers;

[ApiController]
public abstract class ApiController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Диспетчер MediatR для відправки команд (Commands) та запитів (Queries) до їхніх обробників.
    /// </summary>
    protected readonly ISender Mediator = mediator;
    
    protected IActionResult Problem(List<Error>? errors)
    {
        if (errors is null || errors.Count == 0)
        {
            return Problem(); // Повертає 500 Internal Server Error за замовчуванням
        }

        // Якщо всі помилки є валідаційними, пакуємо їх у ModelState
        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        // Для інших типів помилок (Conflict, NotFound тощо) беремо першу основну
        return Problem(errors[0]);
    }

    private IActionResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        // Відповідно до RFC 7807:
        // title — це короткий опис типу помилки
        // detail — це конкретна причина (error.Description)
        return Problem(
            statusCode: statusCode,
            title: GetTitleForErrorType(error.Type),
            detail: error.Description
        );
    }

    private IActionResult ValidationProblem(List<Error> errors)
    {
        var modelState = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelState.AddModelError(
                error.Code, // Тут зазвичай передається ім'я властивості (наприклад, "Email")
                error.Description
            );
        }

        return ValidationProblem(modelState);
    }

    private static string GetTitleForErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => "Resource Not Found",
            ErrorType.Validation => "Validation Error",
            ErrorType.Conflict => "Conflict Occurred",
            ErrorType.Unauthorized => "Unauthorized",
            ErrorType.Forbidden => "Forbidden",
            _ => "An unexpected error occurred"
        };
}