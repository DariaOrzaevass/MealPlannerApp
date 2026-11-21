using MealPlanner.Domain.Shared;

namespace MealPlanner.Web.Contracts;

// Which slot of the plan to replace
public sealed record ReplaceDishRequest(
    int DayNumber,
    MealType MealType,
    Guid[]? RejectedDishes = null);
