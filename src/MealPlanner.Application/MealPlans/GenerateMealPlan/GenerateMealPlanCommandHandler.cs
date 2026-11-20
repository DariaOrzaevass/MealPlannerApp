using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Application.Catalog;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;
using Microsoft.Extensions.Options;

namespace MealPlanner.Application.MealPlans.GenerateMealPlan;

// Builds the palnning proble, runs the solver, and saves the result.
// The entire handler follows the sequence of "convert -> solve -> covert back -> save"
// and doesn't contain optimization logic - it is all implemented in the Planning.
// Every step can fail and expected failure is returned as another result.
// Exceptions are only for situation that shouldn't happen in normal excecution.
internal sealed class GenerateMealPlanCommandHandler(
    ICatalogSnapshotProvider snapshots,
    IMealPlanSolverFactory solvers,
    IMealPlanRepository mealPlans,
    IUnitOfWork unitOfWork,
    IOptions<PlanningOptions> options)
    : ICommandHandler<GenerateMealPlanCommand, GenerateMealPlanResponse>
{
    public async Task<Result<GenerateMealPlanResponse>> Handle(
        GenerateMealPlanCommand command,
        CancellationToken cancellationToken)
    {
        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);

        // 1. Constraints are validated in Domain not in this handler
        Result<PlanningConstraints> constraints = command.Constraints.ToConstraints();

        if (constraints.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(constraints.Error);
        }

        // 2. Converts constraints into planning specification of Planning
        Result<PlanningSpec> spec = PlanningSpecFactory.Create(constraints.Value, snapshot);

        if (spec.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(spec.Error);
        }

        Result<IMealPlanSolver> solver = solvers.Create(command.Algorithm);

        if (solver.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(solver.Error);
        }

        // 3. Planning problem. The index and similarity table are reused from snapshot.
        PlanningProblem problem = PlanningProblem.Create(
            snapshot.Catalog,
            spec.Value,
            seed: options.Value.Seed,
            index: snapshot.Index,
            similarity: snapshot.Similarity);

        if (problem.Domains.HasEmptyDomain)
        {
            // The empty domain of candidates = the result is known without the search
            return Result.Failure<GenerateMealPlanResponse>(PlanningErrors.NoCandidates);
        }

        // 4. The search
        PlanSolution solution = solver.Value.Solve(problem, cancellationToken);

        if (!solution.IsSolved)
        {
            return Result.Failure<GenerateMealPlanResponse>(
                solution.Status == PlanningStatus.Infeasible
                    ? PlanningErrors.Infeasible
                    : PlanningErrors.Exhausted);
        }

        // 5. Converting the solution back. Dish indices into domain identifiers, slots into days and meal types
        MealPlanEntryDraft[] drafts = ToDrafts(solution, spec.Value, snapshot);

        var generationMs = (long)solution.Elapsed.TotalMilliseconds;

        Result<MealPlan> plan = MealPlan.Create(
            command.UserId,
            command.Name,
            command.StartDate,
            constraints.Value,
            drafts,
            command.Algorithm,
            generationMs,
            solution.ExploredNodes);

        if (plan.IsFailure)
        {
            return Result.Failure<GenerateMealPlanResponse>(plan.Error);
        }

        mealPlans.Add(plan.Value);

        // Domain events are dispatched here as part of changes saving.
        // MealPlan.Create raises MealPlanGenerateDomainEvent.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new GenerateMealPlanResponse(
            plan.Value.Id.Value,
            solver.Value.Name,
            plan.Value.TotalCost.Amount,
            solution.BudgetUsage(spec.Value),
            solution.DiversityIndex,
            generationMs,
            solution.ExploredNodes,
            solution.IsProvenOptimal);
    }

    // Converts assignment into meal plan entry drafts. The cost is taken
    // from the snapshot and stored directly into meal plan entry, not
    // calculated from the dish later. Dish prices can change over time,
    // but a saved plan must save its price on the moment when it was generated. 
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
}
