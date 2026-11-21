using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;

namespace MealPlanner.Application.MealPlans.SetEntryLock;

// Sets the lock state of the plan entry
internal sealed class SetEntryLockCommandHandler(
    IMealPlanRepository mealPlans,
    IUnitOfWork unitOfWork) : ICommandHandler<SetEntryLockCommand>
{
    public async Task<Result> Handle(
        SetEntryLockCommand command,
        CancellationToken cancellationToken)
    {
        var planId = MealPlanId.From(command.MealPlanId);

        MealPlan? plan = await mealPlans.GetByIdAsync(planId, cancellationToken);

        if (plan is null || !plan.IsOwnedBy(command.UserId))
        {
            return Result.Failure(MealPlanErrors.NotFound(planId));
        }

        Result result = command.Locked
            ? plan.LockEntry(command.DayNumber, command.MealType)
            : plan.UnlockEntry(command.DayNumber, command.MealType);

        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
