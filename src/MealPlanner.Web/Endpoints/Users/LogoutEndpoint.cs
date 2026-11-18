using Microsoft.AspNetCore.Identity;

namespace MealPlanner.Web.Endpoints.Users;

// Logout endpoint
internal sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("auth/logout", async (SignInManager<IdentityUser> signInManager) =>
        {
            await signInManager.SignOutAsync();

            return Results.NoContent();
        })
        .WithName("Logout")
        .WithSummary("Logout")
        .WithDescription("Removes the authentication cookie.")
        .WithTags(Tags.Users)
        .Produces(StatusCodes.Status204NoContent);
}
