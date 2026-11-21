using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Application.MealPlans.GenerateMealPlan;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Shared;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;

namespace MealPlanner.Application.MealPlans.RegenerateMealPlan;

// Regenerates the plan except fot the locked entries.
// The planning problem is reconstructed from saved plan similarly to
// when 1 dish is replaced, but with additions that locked slots do not
// change, while dishes from unlocked slots are added to the excluded ones.
internal sealed class RegenerateMealPlanCommandHandler(
    ICatalogSnapshotProvider snapshots,
    IMealPlanSolverFactory solvers,
    IMealPlanRepository mealPlans,
    IUnitOfWork unitOfWork) : ICommandHandler<RegenerateMealPlanCommand, GenerateMealPlanResponse>
{
    public async Task<Result<GenerateMealPlanResponse>> Handle(
        RegenerateMealPlanCommand command,
        CancellationToken cancellationToken)
    {
        var planId = MealPlanId.From(command.MealPlanId);

        MealPlan? plan = await mealPlans.GetByIdAsync(planId, cancellationToken);

        if (plan is null || !plan.IsOwnedBy(command.UserId))
        {
            return Result.Failure<GenerateMealPlanResponse>(MealPlanErrors.NotFound(planId));
        }

        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);

        PlannerAlgorithm algorithm = command.Algorithm ?? plan.Algorithm;

        Result<IMealPlanSolver> solver = solvers.Create(algorithm);

        if (solver.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(solver.Error);
        }

        Result<Layout> layout = BuildLayout(plan, snapshot);

        if (layout.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(layout.Error);
        }

        Result<PlanningSpec> spec = PlanningSpecFactory.Create(
            plan.Constraints,
            snapshot,
            excludedDishes: layout.Value.Excluded,
            pinnedSlots: layout.Value.Pinned);

        if (spec.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(spec.Error);
        }

        PlanningProblem problem = PlanningProblem.Create(
            snapshot.Catalog,
            spec.Value,
            index: snapshot.Index,
            similarity: snapshot.Similarity);

        if (problem.Domains.HasEmptyDomain)
        {
            return Result.Failure<GenerateMealPlanResponse>(PlanningErrors.NothingLeftToOffer);
        }

        PlanSolution solution = solver.Value.Solve(problem, cancellationToken);

        if (!solution.IsSolved)
        {
            return Result.Failure<GenerateMealPlanResponse>(
                solution.Status == PlanningStatus.Infeasible
                    ? PlanningErrors.Infeasible
                    : PlanningErrors.Exhausted);
        }

        var generationMs = (long)solution.Elapsed.TotalMilliseconds;

        Result replanned = plan.Replan(
            ToDrafts(solution, spec.Value, snapshot),
            algorithm,
            generationMs,
            solution.ExploredNodes);

        if (replanned.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(replanned.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new GenerateMealPlanResponse(
            plan.Id.Value,
            solver.Value.Name,
            plan.TotalCost.Amount,
            solution.BudgetUsage(spec.Value),
            solution.DiversityIndex,
            generationMs,
            solution.ExploredNodes,
            solution.IsProvenOptimal);
    }

    private static Result<Layout> BuildLayout(MealPlan plan, CatalogSnapshot snapshot)
    {
        var pinned = new Dictionary<int, int>();
        var excluded = new List<int>();

        int mealsPerDay = plan.Constraints.MealsPerDay.Count;

        foreach (MealPlanEntry entry in plan.Entries)
        {
            if (!snapshot.IndexByDishId.TryGetValue(entry.DishId, out int dish))
            {
                return Result.Failure<Layout>(PlanningErrors.DishOutOfCatalog);
            }

            int mealIndex = IndexOfMeal(plan.Constraints.MealsPerDay, entry.MealType);

            if (mealIndex < 0)
            {
                return Result.Failure<Layout>(MealPlanErrors.SlotOutOfRange);
            }

            int slot = ((entry.DayNumber - 1) * mealsPerDay) + mealIndex;

            if (entry.IsLocked)
            {
                pinned[slot] = dish;
            }
            else
            {
                excluded.Add(dish);
            }
        }

        return new Layout(pinned, excluded);
    }

    private static int IndexOfMeal(IReadOnlyList<MealType> meals, MealType meal)
    {
        for (int i = 0; i < meals.Count; i++)
        {
            if (meals[i] == meal)
            {
                return i;
            }
        }

        return -1;
    }

    private static MealPlanEntryDraft[] ToDrafts(
        PlanSolution solution,
        PlanningSpec spec,
        CatalogSnapshot snapshot)
    {
        var drafts = new MealPlanEntryDraft[solution.Assignment.Count];

        for (int slot = 0; slot < drafts.Length; slot++)
        {
            int dish = solution.Assignment[slot];
            SlotDescriptor descriptor = spec.Slots[slot];

            drafts[slot] = new MealPlanEntryDraft(
                descriptor.Day,
                CatalogSnapshotBuilder.ToMealType(descriptor.Kind),
                snapshot.DishIds[dish],
                snapshot.CostOf(dish));
        }

        return drafts;
    }

    private readonly record struct Layout(
        IReadOnlyDictionary<int, int> Pinned,
        IReadOnlyList<int> Excluded);
}
