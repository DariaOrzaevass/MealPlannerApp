using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;
using Microsoft.Extensions.Options;

namespace MealPlanner.Application.MealPlans.ReplaceDish;

// Finds the closest feasible replacement and performs the replacement.
// The planning problem is reconstructed from the saved meal plan
// (constraints from the plan + assignment from saved entried).
// Two checks one for planning constraints like allergens, calories, etc.
// and one for aggregate invariants - locked entries and plan budget.
internal sealed class ReplaceDishCommandHandler(
    ICatalogSnapshotProvider snapshots,
    IMealPlanRepository mealPlans,
    IUnitOfWork unitOfWork,
    IOptions<PlanningOptions> options)
    : ICommandHandler<ReplaceDishCommand, ReplaceDishResponse>
{
    public async Task<Result<ReplaceDishResponse>> Handle(
    ReplaceDishCommand command,
    CancellationToken cancellationToken)
    {
        var planId = MealPlanId.From(command.MealPlanId);

        MealPlan? plan = await mealPlans.GetByIdAsync(planId, cancellationToken);

        if (plan is null || !plan.IsOwnedBy(command.UserId))
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.NotFound(planId));
        }

        MealPlanEntry? entry = plan.FindEntry(command.DayNumber, command.MealType);

        if (entry is null)
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.EntryNotFound);
        }

        if (entry.IsLocked)
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.EntryLocked);
        }

        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);

        Result<PlanningSpec> spec = PlanningSpecFactory.Create(plan.Constraints, snapshot);

        if (spec.IsFailure)
        {
            return Result.Failure<ReplaceDishResponse>(spec.Error);
        }

        Result<int[]> assignment = RestoreAssignment(plan, spec.Value, snapshot);

        if (assignment.IsFailure)
        {
            return Result.Failure<ReplaceDishResponse>(assignment.Error);
        }

        int slotIndex = SlotIndexOf(spec.Value, command.DayNumber, command.MealType);

        if (slotIndex < 0)
        {
            return Result.Failure<ReplaceDishResponse>(MealPlanErrors.EntryNotFound);
        }

        PlanningProblem problem = PlanningProblem.Create(
            snapshot.Catalog,
            spec.Value,
            seed: options.Value.Seed,
            index: snapshot.Index,
            similarity: snapshot.Similarity);

        var finder = new SubstitutionFinder(options.Value.HeapArity);

        HashSet<int> rejected =
        [
            .. command.RejectedDishes
                .Select(DishId.From)
                .Where(snapshot.IndexByDishId.ContainsKey)
                .Select(id => snapshot.IndexByDishId[id])
        ];

        SubstitutionResult substitution = finder.Find(
            problem,
            snapshot.SimilarityGraph,
            assignment.Value,
            slotIndex,
            rejected,
            cancellationToken);

        if (!substitution.Found)
        {
            return Result.Failure<ReplaceDishResponse>(
                PlanningErrors.NoSubstitution(substitution.FailureReason ?? "the reason is unknown"));
        }

        DishId oldDishId = entry.DishId;
        string oldName = NameOf(oldDishId, snapshot);

        DishId newDishId = snapshot.DishIds[substitution.Dish];
        Money newCost = snapshot.CostOf(substitution.Dish);

        Result replaced = plan.ReplaceDish(
            command.DayNumber,
            command.MealType,
            newDishId,
            newCost);

        if (replaced.IsFailure)
        {
            return Result.Failure<ReplaceDishResponse>(replaced.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReplaceDishResponse(
            oldDishId.Value,
            oldName,
            newDishId.Value,
            snapshot.DishNames[substitution.Dish],
            newCost.Amount,
            plan.TotalCost.Amount,
            substitution.Distance,
            substitution.Path.Count,
            substitution.ExploredNodes);
    }

    private static Result<int[]> RestoreAssignment(
        MealPlan plan,
        PlanningSpec spec,
        CatalogSnapshot snapshot)
    {
        var assignment = new int[spec.SlotCount];

        Array.Fill(assignment, -1);

        foreach (MealPlanEntry entry in plan.Entries)
        {
            int slot = SlotIndexOf(spec, entry.DayNumber, entry.MealType);

            if (slot < 0)
            {
                return Result.Failure<int[]>(MealPlanErrors.SlotOutOfRange);
            }

            if (!snapshot.IndexByDishId.TryGetValue(entry.DishId, out int dish))
            {
                return Result.Failure<int[]>(PlanningErrors.DishOutOfCatalog);
            }

            assignment[slot] = dish;
        }

        return Array.Exists(assignment, d => d < 0)
            ? Result.Failure<int[]>(MealPlanErrors.SlotCountMismatch)
            : assignment;
    }

    private static int SlotIndexOf(PlanningSpec spec, int dayNumber, MealType mealType)
    {
        MealKinds kind = CatalogSnapshotBuilder.ToMealKind(mealType);

        for (int i = 0; i < spec.Slots.Count; i++)
        {
            SlotDescriptor slot = spec.Slots[i];

            if (slot.Day == dayNumber && slot.Kind == kind)
            {
                return i;
            }
        }

        return -1;
    }

    private static string NameOf(DishId dishId, CatalogSnapshot snapshot) =>
        snapshot.IndexByDishId.TryGetValue(dishId, out int index)
        ? snapshot.DishNames[index]
        : "(dish is no longer published)";
}
