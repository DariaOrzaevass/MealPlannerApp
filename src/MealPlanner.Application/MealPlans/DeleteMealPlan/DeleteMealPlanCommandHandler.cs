using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;

namespace MealPlanner.Application.MealPlans.DeleteMealPlan;

// Delets the plan with its entries, becasue they belong to
// the meal plan as an owned collection and have the same lifetime.
internal sealed class DeleteMealPlanCommandHandler(
    IMealPlanRepository mealPlans,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteMealPlanCommand>
{
    public async Task<Result> Handle(
        DeleteMealPlanCommand command,
        CancellationToken cancellationToken)
    {
        var planId = MealPlanId.From(command.MealPlanId);

        MealPlan? plan = await mealPlans.GetByIdAsync(planId, cancellationToken);

        if (plan is null || !plan.IsOwnedBy(command.UserId))
        {
            return Result.Failure(MealPlanErrors.NotFound(planId));
        }

        mealPlans.Remove(plan);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
