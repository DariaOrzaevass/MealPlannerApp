using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.Abstractions.Authentication;

// For working with ASP.NET Core Identity from Application layer
public interface IIdentityProvider
{
    // Create account for user and return its idntifier
    Task<Result<string>> CreateAsync(
        string email,
        string password,
        string role,
        CancellationToken cancellationToken = default);

    // Delete user account - for failed registration
    Task DeleteAsync(string identityId, CancellationToken cancellationToken = default);
}
