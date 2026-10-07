using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans.Events;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Users;

namespace MealPlanner.Domain.MealPlans;

// A Meal Plan entity representing a specific Meal Plan
public sealed class MealPlan : Entity<MealPlanId>
{
    // The list of all the items in the meal plan
    private readonly List<MealPlanEntry> _entries = [];

    // Private constructor so that the dish can only be created using the Create method with validation
    private MealPlan(
    MealPlanId id,
    UserId userId,
    string name,
    DateOnly startDate,
    PlanningConstraints constraints,
    PlannerAlgorithm algorithm,
    long generationMs,
    long exploredNodes)
    : base(id)
    {
        UserId = userId;
        Name = name;
        StartDate = startDate;
        Constraints = constraints;
        Algorithm = algorithm;
        CreatedOnUtc = DateTime.UtcNow;
        GenerationMs = generationMs;
        ExploredNodes = exploredNodes;
        TotalCost = Money.Zero(constraints.Budget.Currency);
    }

    // Default constructor for EF core to get the Meal Plan from the database
    private MealPlan()
    {
    }

    // Id of the user to whom the meal plan belongs to
    public UserId UserId { get; private set; } = null!;

    // Name of the meal plan
    public string Name { get; private set; } = null!;

    // When the meal olan begins
    public DateOnly StartDate { get; private set; }

    // Constraints of the plan (budget and etc.)
    public PlanningConstraints Constraints { get; private set; } = null!;

    // The algorithm that was used for creation of this meal plan
    public PlannerAlgorithm Algorithm { get; private set; }

    // When the plan was created
    public DateTime CreatedOnUtc { get; private set; }

    // For how long (in ms) the plan was generating. For the later comparison of the algorithms performance
    public long GenerationMs { get; private set; }

    // The amount of explored nodes, needed for algorithms
    public long ExploredNodes { get; private set; }

    // Total cost of the meal plan
    public Money TotalCost { get; private set; } = null!;

    // The read-only collection of entries(dishes) of the meal plan
    public IReadOnlyCollection<MealPlanEntry> Entries => _entries.AsReadOnly();

    // Creates a new meal plan and raises a domain event after checking the input
    public static Result<MealPlan> Create(
        UserId userId,
        string name,
        DateOnly startDate,
        PlanningConstraints constraints,
        IReadOnlyCollection<MealPlanEntryDraft> drafts,
        PlannerAlgorithm algorithm,
        long generationMs,
        long exploredNodes = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<MealPlan>(MealPlanErrors.EmptyName);
        }

        Result layout = ValidateLayout(constraints, drafts);

        if (layout.IsFailure)
        {
            return Result.Failure<MealPlan>(layout.Error);
        }

        var plan = new MealPlan(
            MealPlanId.New(),
            userId,
            name.Trim(),
            startDate,
            constraints,
            algorithm,
            generationMs,
            exploredNodes);

        foreach (MealPlanEntryDraft draft in drafts)
        {
            plan._entries.Add(new MealPlanEntry(
                MealPlanEntryId.New(),
                plan.Id,
                draft.DayNumber,
                draft.MealType,
                draft.DishId,
                draft.Cost));
        }

        plan.RecalculateTotal();
        plan.RaiseDomainEvent(new MealPlanGeneratedDomainEvent(plan.Id, algorithm, generationMs));

        return plan;
    }

    // Recalculates the total cost of the plan
    private void RecalculateTotal() =>
    TotalCost = _entries.Aggregate(
        Money.Zero(Constraints.Budget.Currency),
        (acc, entry) => acc + entry.Cost);

    // Validates the input
    private static Result ValidateLayout(
        PlanningConstraints constraints,
        IReadOnlyCollection<MealPlanEntryDraft> drafts)
    {
        if (drafts.Count != constraints.SlotCount)
        {
            return Result.Failure(MealPlanErrors.SlotCountMismatch);
        }

        // Set that stores unique combinations of a day (1, 2...) and
        // a type of meal (Breakfast etc.) with capacity of the number of drafts (drafts.Count)
        var seen = new HashSet<(int Day, MealType Meal)>(drafts.Count);

        foreach (MealPlanEntryDraft draft in drafts)
        {
            if (draft.DayNumber < 1 || draft.DayNumber > constraints.DurationDays)
            {
                return Result.Failure(MealPlanErrors.SlotOutOfRange);
            }

            if (!constraints.MealsPerDay.Contains(draft.MealType))
            {
                return Result.Failure(MealPlanErrors.SlotOutOfRange);
            }

            if (!seen.Add((draft.DayNumber, draft.MealType)))
            {
                return Result.Failure(MealPlanErrors.DuplicateSlot);
            }
        }

        return Result.Success();
    }
}
