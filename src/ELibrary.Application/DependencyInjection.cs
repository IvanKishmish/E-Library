using ELibrary.Application.Common.Behaviours;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ELibrary.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
 
        // Registers all IRequestHandler<,> implementations found in this assembly.
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
 
            // Attach the validation pipeline so every command/query is validated
            // before reaching its handler.
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
 
        // Scans the assembly and registers all AbstractValidator<T> implementations
        // with a Scoped lifetime (FluentValidation's recommended default).
        services.AddValidatorsFromAssembly(assembly);
 
        return services;
    }
}