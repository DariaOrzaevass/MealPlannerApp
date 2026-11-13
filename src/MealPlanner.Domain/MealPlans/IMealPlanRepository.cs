using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Users;

namespace MealPlanner.Domain.MealPlans;

// Repository interface for MealPlan
public interface IMealPlanRepository
{
    Task<MealPlan?> GetByIdAsync(MealPlanId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<MealPlan>> GetByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    void Add(MealPlan mealPlan);

    void Remove(MealPlan mealPlan);
}
