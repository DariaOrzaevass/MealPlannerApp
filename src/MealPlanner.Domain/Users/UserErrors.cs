using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Users;

// Class containing all errors related to the user
public static class UserErrors
{
    // Returns an error if the user with this ID is not found
    public static Error NotFound(UserId id) =>
        Error.NotFound("User.NotFound", $"User with ID {id} not found.");

    // Returns an error if the email is empty
    public static readonly Error EmptyEmail =
        Error.Validation("User.EmptyEmail", "Email address cannot be empty.");

    // Returns an error if the identityId is empty
    public static readonly Error EmptyIdentityId =
        Error.Validation("User.EmptyIdentityId", "Identity ID cannot be empty.");

    // Returns an error when no domain user corresponds to the current account
    public static readonly Error NotAuthenticated =
        Error.NotFound("User.NotAuthenticated", "The current user is not defined.");

    // Returns an error if the plan does not belong to this user
    public static readonly Error NotOwner =
        Error.Problem("User.NotOwner", "The plan belongs to another user.");

}
