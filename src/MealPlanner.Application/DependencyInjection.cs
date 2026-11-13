using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Application;

// Registration of supportiong services for the Application layer
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        return services;
    }
}
