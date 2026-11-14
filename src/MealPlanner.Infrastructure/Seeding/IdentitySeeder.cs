using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Users;
using MealPlanner.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Infrastructure.Seeding;

// Creates roles and the first administrator
public sealed class IdentitySeeder(
    RoleManager<IdentityRole> roles,
    IIdentityProvider identity,
    IUserRepository users,
    ApplicationDbContext context,
    ILogger<IdentitySeeder> logger)
{
    // Creates all required roles
    public async Task SeedRolesAsync(CancellationToken cancellationToken = default)
    {
        foreach (string role in Roles.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole(role));

                logger.LogInformation("Role {Role} was created.", role);
            }
        }
    }

    // Creates firsr administrator account
    public async Task SeedAdministratorAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        bool exists = await context.Set<User>()
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (exists)
        {
            return;
        }

        Result<string> identityId = await identity.CreateAsync(
            email,
            password,
            Roles.Administrator,
            cancellationToken);

        if (identityId.IsFailure)
        {
            logger.LogWarning(
                "Failed to create the administrator: {Error}",
                identityId.Error.Description);

            return;
        }

        users.Add(User.Create(email, identityId.Value).Value);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Administrator {Email} was created.", email);
    }
}
