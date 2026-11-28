using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FluentValidation;
using MealPlanner.Application.Abstractions.Decorators;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Application.MealPlans;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Application;

// Registration of supporting services for the Application layer
public static class DependencyInjection
{
    // Reference to assembly containing the application layer to only search in this assembly
    private static readonly Assembly _applicationAssembly = typeof(DependencyInjection).Assembly;

    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<ICatalogSnapshotProvider, CatalogSnapshotProvider>();

        services.Configure<PlanningOptions>(
            configuration.GetSection(PlanningOptions.SectionName));

        services.AddSingleton<IMealPlanSolverFactory, MealPlanSolverFactory>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        AddHandlers(services);
        AddDecorators(services);

        // Searches for validators in the same assembly. includeInternalTypes needed because
        // handlers & validators are internal and shouldn't be visible outside the Application layer
        services.AddValidatorsFromAssembly(_applicationAssembly, includeInternalTypes: true);

        return services;
    }

    // Finds and registers all command/query/domain event handlers
    private static void AddHandlers(IServiceCollection services) =>
        services.Scan(scan => scan
        .FromAssemblies(_applicationAssembly)
        .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
        .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
        .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime()
        .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)), publicOnly: false)
        .AsImplementedInterfaces()
        .WithScopedLifetime());

    private static void AddDecorators(IServiceCollection services)
    {
        services.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandHandler<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(ValidationDecorator.QueryHandler<,>));

        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandler<>));
        services.TryDecorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandler<,>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));
    }
}
