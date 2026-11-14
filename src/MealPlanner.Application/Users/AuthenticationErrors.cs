using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.Users;

// Errors related to registration or login
public static class AuthenticationErrors
{
    // There is an account with this email already
    public static readonly Error EmailTaken = Error.Conflict(
        "Authentication.EmailTaken",
        "A user with this email already exists.");

    // Deliberately only one error for both cases. Separate responses
    // "user does not exist" and "incorrect password" would allow
    // to determine registered users by trying different emails.
    public static readonly Error InvalidCredentials = Error.Problem(
        "Authentication.InvalidCredentials",
        "Wrong email or password.");

    // Password does not fit Identity requirements. Meal Planner does
    // not have such requirements, so we just take explanaitions
    // from Identity and inserts into error description.
    public static Error WeakPassword(string reason) => Error.Validation(
        "Authentication.WeakPassword",
        reason);

    // A general Identity failure that doesn't fit previous cases
    public static Error Failed(string reason) => Error.Failure(
        "Authentication.Failed",
        reason);
}
