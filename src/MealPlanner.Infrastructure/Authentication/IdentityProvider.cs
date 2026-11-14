using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Users;
using MealPlanner.Domain.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Infrastructure.Authentication;

// Implements IIdentityProvider interface from Application layer
internal sealed class IdentityProvider(UserManager<IdentityUser> users) : IIdentityProvider
{
    public async Task<Result<string>> CreateAsync(
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var identityUser = new IdentityUser
        {
            Email = email,
            UserName = email
        };

        IdentityResult created = await users.CreateAsync(identityUser, password);

        if (!created.Succeeded)
        {
            return Result.Failure<string>(Translate(created));
        }

        IdentityResult roleAssigned = await users.AddToRoleAsync(identityUser, role);

        if (!roleAssigned.Succeeded)
        {
            // Account without a role is useless, it won't pass authorization checks.
            // Therefore it is deleted immediately to not to have incomplete account.
            await users.DeleteAsync(identityUser);

            return Result.Failure<string>(Translate(roleAssigned));
        }

        return identityUser.Id;
    }

    public async Task DeleteAsync(string identityId, CancellationToken cancellationToken = default)
    {
        IdentityUser? identityUser = await users.FindByIdAsync(identityId);

        if (identityUser is not null)
        {
            await users.DeleteAsync(identityUser);
        }
    }

    // Turns the Identity errors into Meal Planner App error
    // Error codes are interpreted, not copied directly.
    private static Error Translate(IdentityResult result)
    {
        IdentityError[] errors = [.. result.Errors];

        if (Array.Exists(errors, e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
        {
            return AuthenticationErrors.EmailTaken;
        }

        if (Array.Exists(errors, e => e.Code.StartsWith("Password", StringComparison.Ordinal)))
        {
            return AuthenticationErrors.WeakPassword(
                string.Join(" ", errors.Select(e => e.Description)));
        }

        return AuthenticationErrors.Failed(
            string.Join(" ", errors.Select(e => e.Description)));
    }
}
