using MealPlanner.Infrastructure.Authentication;
using MealPlanner.Infrastructure.Database;
using MealPlanner.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Web.Extensions;

// Prepares the database when application starts
public static class DatabaseStartupExtensions
{
    // Applies migrations and fill the catalog with data
    public static async Task PrepareDatabaseAsync(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        await using AsyncServiceScope scope = app.Services.CreateAsyncScope();

        ApplicationDbContext context =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.MigrateAsync();

        // The identity schema belongs to its own context and is migrated separately
        ApplicationIdentityDbContext identityContext =
            scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        await identityContext.Database.MigrateAsync();

        CatalogSeeder seeder = scope.ServiceProvider.GetRequiredService<CatalogSeeder>();

        await seeder.SeedAsync();

        IdentitySeeder identitySeeder = scope.ServiceProvider.GetRequiredService<IdentitySeeder>();

        // Roles are always needed, because without them authorization checks cannot
        // succeed and roles cannot be created initially through the user interface,
        // because doing that also requires permissions that no user has yet.
        await identitySeeder.SeedRolesAsync();

        // Administrator is created only if password is provided through Configuration,
        // because hard-coding account password in the source code - security vulnerability
        string? adminEmail = app.Configuration["Seed:AdministratorEmail"];
        string? adminPassword = app.Configuration["Seed:AdministratorPassword"];

        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
        {
            await identitySeeder.SeedAdministratorAsync(adminEmail, adminPassword);
        }
    }
}
