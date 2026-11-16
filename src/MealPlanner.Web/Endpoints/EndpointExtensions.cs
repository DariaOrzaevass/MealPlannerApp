using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MealPlanner.Web.Endpoints;

// Automatically finds, registers, and mapps all endpoints of Web layer
public static class EndpointExtensions
{
    // Finds all implementation of IEndpoint and registers them in dependency
    // injection. The endpoint object is only needed once during application
    // startup to call its MapEndpoint method. After that, the route handlers
    // themselves are used and the endpoint object is no longer needed -> transient
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        ServiceDescriptor[] endpoints =
        [
            .. Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type is { IsAbstract: false, IsInterface: false }
                               && type.IsAssignableTo(typeof(IEndpoint)))
                .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
        ];

        // TryAddEnumerable avoids registering the same implementation more than once.
        // If the registration method were accidentally called twice using a normal Add,
        // all routes could be registered twice, which could lead to ambiguous routing.
        services.TryAddEnumerable(endpoints);

        return services;
    }

    // Maps all discovered endpoints. A route group can be supplied if the
    // endpoints should share a common prefix or authorization policy.
    // Without one, they are mapped directly to the application root.
    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? group = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        IEndpointRouteBuilder builder = group is null ? app : group;

        foreach (IEndpoint endpoint in app.Services.GetRequiredService<IEnumerable<IEndpoint>>())
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
