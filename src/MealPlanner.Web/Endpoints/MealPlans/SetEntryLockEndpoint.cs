using MealPlanner.Application.Abstractions.Authentication;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.MealPlans.SetEntryLock;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Users;
using MealPlanner.Web.Extensions;

namespace MealPlanner.Web.Endpoints.MealPlans;

// Lock or unlock the meal plan entry
internal sealed class SetEntryLockEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app) =>
        app.MapPut("meal-plans/{id:guid}/entries/lock", async (
            Guid id,
            LockRequest request,
            IUserContext userContext,
            ICommandHandler<SetEntryLockCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result<UserId> userId = await userContext.GetUserIdAsync(cancellationToken);

            if (userId.IsFailure)
            {
                return ApiResults.Problem(userId);
            }

            Result result = await handler.Handle(
                new SetEntryLockCommand(
                    userId.Value, id, request.DayNumber, request.MealType, request.Locked),
                cancellationToken);

            return result.IsSuccess ? Results.NoContent() : ApiResults.Problem(result);
        })
        .WithName("SetEntryLock")
        .WithSummary("Lock or unlock the meal plan entry")
        .WithDescription(
            "The locked meal plan entry is not changed during the dish replacement of the regeneratio of the meal plan.")
        .WithTags(Tags.MealPlans)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

    internal sealed record LockRequest(int DayNumber, MealType MealType, bool Locked);
}
