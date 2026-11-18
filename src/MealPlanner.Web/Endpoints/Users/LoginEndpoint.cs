using MealPlanner.Application.Users;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Web.Contracts;
using MealPlanner.Web.Extensions;
using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Web.Endpoints.Users;

// Signs in using an email address and password.
// This is the only endpoint that does not work through a command handler.
// Signing in does not change the state of the application domain.
// Creating a command for this operation would require passing HttpContext
// into the Application layer, which is exactly the type of dependency that
// the layer separation is intended to prevent.
internal sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
    app.MapPost("auth/login", async (
        LoginRequest request,
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager) =>
    {
        IdentityUser? user = await userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return ApiResults.Problem(
                Result.Failure(AuthenticationErrors.InvalidCredentials));
        }

        SignInResult signIn = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,

            // Failed login attempts are counted. Without this the
            // password can be guessed repeatedly without restriction
            lockoutOnFailure: true);

        if (signIn.IsLockedOut)
        {
            return ApiResults.Problem(Result.Failure(
                Error.Problem(
                    "Authentication.LockedOut",
                    "Too many failed attempts. Please try again later.")));
        }

        // The same error is returned for "user does not exist" and "incorrect password":
        // different responses could allow to determine who is registered.
        return signIn.Succeeded
            ? Results.NoContent()
            : ApiResults.Problem(Result.Failure(AuthenticationErrors.InvalidCredentials));
    })
    .AllowAnonymous()
    .WithName("Login")
    .WithSummary("Login")
    .WithDescription("Validates the password and issues an authentication cookie.")
    .WithTags(Tags.Users)
    .Produces(StatusCodes.Status204NoContent)
    .ProducesProblem(StatusCodes.Status400BadRequest);
}
