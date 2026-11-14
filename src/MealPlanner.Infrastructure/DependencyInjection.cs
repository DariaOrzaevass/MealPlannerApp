using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Authentication;
using MealPlanner.Infrastructure.Database;
using MealPlanner.Infrastructure.Repositories;
using MealPlanner.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MealPlanner.Infrastructure;

// Registration of supporting services for the Infrastructure layer
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        string connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "The 'Database' connection string is not configured."
                + "Set the ConnectionStrings__Database environment variable.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
                // A connection to the database in the container might not succeed from
                // the first try if SQL Server is still starting. By retrying temporary
                // failures are avoided, but permanent database unavailability isn't solved.
                sqlServer.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null)));

        // Second context for users accounts
        services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);

                // Has its own Migrations History Table in its own schema. Otherwise both contexts
                // would write to the same dbo.__EFMigrationsHistory and see each other's records.
                // The table would become a shared changeable resource for two independant
                // migration sets, which is against the purpose of separating them.
                sqlServer.MigrationsHistoryTable(
                    "__EFMigrationsHistory",
                    ApplicationIdentityDbContext.Schema);
            }));

        // Context already implements IUnitOfWork, so the same instance is
        // registered without creating a second object, in which case changes
        // could be commited through a different Unit of Work transaction.
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IAllergenRepository, AllergenRepository>();
        services.AddScoped<IIngredientRepository, IngredientRepository>();
        services.AddScoped<IDishRepository, DishRepository>();
        services.AddScoped<IMealPlanRepository, MealPlanRepository>();


        services.AddScoped<IUserRepository, UserRepository>();

        services.AddScoped<CatalogSeeder>();
        services.AddScoped<IdentitySeeder>();

        services.AddScoped<IIdentityProvider, IdentityProvider>();

        return services;
    }
}
