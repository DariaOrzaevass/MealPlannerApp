using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.MealPlans;

// Errors occuring in conversion of domain planning constraints into planning
// problem. They are separate from MealPlanErrors, because do not represent
// violations of domain rules, but failures of conversion process.
public static class PlanningErrors
{
    public static readonly Error CurrencyMismatch = Error.Problem(
        "Planning.CurrencyMismatch",
        "The budget currency does not match the catalog currency.");

    public static readonly Error UnknownCurrency = Error.Problem(
        "Planning.UnknownCurrency",
        "An unknown currency code.");

    public static readonly Error UnknownAllergen = Error.Problem(
        "Planning.UnknownAllergen",
        "The constraints contain nonexisting allergen.");

    public static readonly Error UnknownIngredient = Error.Problem(
        "Planning.UnknownIngredient",
        "The constraints contain nonexisting ingredient.");

    public static readonly Error BudgetTooSmall = Error.Problem(
        "Planning.BudgetTooSmall",
        "The budget is lower than the cost of the cheapest possible set of dishes.");

    public static readonly Error UnknownAlgorithm = Error.Problem(
        "Planning.UnknownAlgorithm",
        "An unknown meal plan generation algorithm was requested.");

    // The solver has no candidates for one of the slot domains
    public static readonly Error NoCandidates = Error.Problem(
        "Planning.NoCandidates",
        "No dishes satisfy specified constraints.");

    // The solver completed the search and proved that no solution exists
    public static readonly Error Infeasible = Error.Problem(
        "Planning.Infeasible",
        "It is impossible to generate a plan with specified constraints.");

    // The plan references an unpublished dish
    public static readonly Error DishOutOfCatalog = Error.Problem(
        "Planning.DishOutOfCatalog",
        "The plan consists a dish that is no longer published. A replacement cannot be generated.");

    public static Error NoSubstitution(string reason) => Error.Problem(
        "Planning.NoSubstitution",
        $"No suitable replacement was found: {reason}");

    // Regeneration exhausted all available alternatives for dishes
    public static readonly Error NothingLeftToOffer = Error.Problem(
        "Planning.NothingLeftToOffer",
        "No other options remain: only already displayed dishes satisfy these constraints.");

    // The solver exhausted its time or node limit
    public static readonly Error Exhausted = Error.Problem(
        "Planning.Exhausted",
        "The solver did not finish in the allowed time. Try relaxing the constraints.");
}
