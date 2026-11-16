using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Users.RegisterUser;
using MealPlanner.Domain.Abstractions;
using Microsoft.AspNetCore.Identity.Data;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.Users;

// Endpoint used for registering a new user
internal sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPost("auth/register", async (
            RegisterRequest request,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(
                new RegisterUserCommand(request.Email, request.Password),
                cancellationToken);

            return result.IsSuccess
                ? Results.Ok(new { userId = result.Value })
                : ApiResults.Problem(result);
        })
        .AllowAnonymous()
        .WithName("Register")
        .WithSummary("Registration")
        .WithDescription(
            "Creates Identity account and a domain user. " +
            "Login after registration is made through a separate request.")
        .WithTags(Tags.Users)
        .Produces<object>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
}
