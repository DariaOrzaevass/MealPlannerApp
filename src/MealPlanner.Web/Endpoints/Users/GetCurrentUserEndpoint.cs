using System.Security.Claims;
using MealPlanner.Web.Contracts;

namespace MealPlanner.Web.Endpoints.Users;

// Who is the current user and and their permissions.
// The interface needs to know whether to display the "Catalog" option, which is
// visible only to administrators. Hiding the button does not replace server-side
// authorization checks, but it prevents users from seeing actions that would be rejected.
internal sealed class GetCurrentUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapGet("auth/me", (ClaimsPrincipal principal) =>
        {
            string email = principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

            string[] roles = [.. principal.FindAll(ClaimTypes.Role).Select(c => c.Value)];

            return Results.Ok(new CurrentUserResponse(email, roles));
        })
        .WithName("GetCurrentUser")
        .WithSummary("Current User")
        .WithDescription("Returns the email and roles of the currently authenticated user.")
        .WithTags(Tags.Users)
        .Produces<CurrentUserResponse>()
        .ProducesProblem(StatusCodes.Status401Unauthorized);
}
