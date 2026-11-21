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

namespace MealPlanner.Application.MealPlans.CompareSolvers;

// Runs all solvers of the same planning problem that is prepared once and
// passed to every solver for fair comparison. If one solver failed,
// the comparison continues, because a failure is a result in itself.
// Failure reasons are also provided by solvers, because they can be unique.
internal sealed class CompareSolversQueryHandler(
    ICatalogSnapshotProvider snapshots,
    IMealPlanSolverFactory solvers,
    IOptions<PlanningOptions> options)
    : IQueryHandler<CompareSolversQuery, IReadOnlyCollection<SolverComparisonRow>>
{
    public async Task<Result<IReadOnlyCollection<SolverComparisonRow>>> Handle(
        CompareSolversQuery query,
        CancellationToken cancellationToken)
    {
        CatalogSnapshot snapshot = await snapshots.GetAsync(cancellationToken);

        Result<PlanningConstraints> constraints = query.Constraints.ToConstraints();

        if (constraints.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<SolverComparisonRow>>(constraints.Error);
        }

        Result<PlanningSpec> spec = PlanningSpecFactory.Create(constraints.Value, snapshot);

        if (spec.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<SolverComparisonRow>>(spec.Error);
        }

        PlanningProblem problem = PlanningProblem.Create(
            snapshot.Catalog,
            spec.Value,
            seed: options.Value.Seed,
            index: snapshot.Index,
            similarity: snapshot.Similarity);

        if (problem.Domains.HasEmptyDomain)
        {
            return Result.Failure<IReadOnlyCollection<SolverComparisonRow>>(PlanningErrors.NoCandidates);
        }

        var rows = new List<SolverComparisonRow>(solvers.Available.Count);

        foreach (PlannerAlgorithm algorithm in solvers.Available.Order())
        {
            cancellationToken.ThrowIfCancellationRequested();

            Result<IMealPlanSolver> solver = solvers.Create(algorithm);

            if (solver.IsFailure)
            {
                continue;
            }

            rows.Add(Run(algorithm, solver.Value, problem, spec.Value, cancellationToken));
        }

        return rows;
    }

    private static SolverComparisonRow Run(
        PlannerAlgorithm algorithm,
        IMealPlanSolver solver,
        PlanningProblem problem,
        PlanningSpec spec,
        CancellationToken cancellationToken)
    {
        PlanSolution solution = solver.Solve(problem, cancellationToken);

        if (!solution.IsSolved)
        {
            return new SolverComparisonRow(
                algorithm,
                solver.Name,
                Solved: false,
                Failure: solution.FailureReason ?? (solution.Status == PlanningStatus.Infeasible
                    ? "the feasible solution does not exist"
                    : "the solver was not able to find a solution"),
                TotalCost: 0m,
                BudgetUsage: 0,
                DiversityIndex: 0,
                ElapsedMs: (long)solution.Elapsed.TotalMilliseconds,
                ExploredNodes: solution.ExploredNodes,
                IsProvenOptimal: false);
        }

        return new SolverComparisonRow(
            algorithm,
            solver.Name,
            Solved: true,
            Failure: null,
            TotalCost: solution.TotalCostMinorUnits / 100m,
            BudgetUsage: solution.BudgetUsage(spec),
            DiversityIndex: solution.DiversityIndex,
            ElapsedMs: (long)solution.Elapsed.TotalMilliseconds,
            ExploredNodes: solution.ExploredNodes,
            IsProvenOptimal: solution.IsProvenOptimal);
    }
}
