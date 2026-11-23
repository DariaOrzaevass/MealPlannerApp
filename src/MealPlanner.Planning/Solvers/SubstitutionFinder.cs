using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// Searches for a substitution of a dish using Dijkstra's algorithm on similarity graph.
// When the user rejects a dish, application searches for a substitution dish, which is
// the closesr according to the similarity - nearest vertex in a graph with non-negative weights.
// Vertives are removed in non-decreasing order of distance, so the first extracter vertex satisfying
// constraints is the nearest valid substitution. Simple minimum search is not sufficient because
// the graph is made sparse using similarity threshold, so rejected dish might not have a direct
// neighbour satisfying all constraints. Also dish A can be similar to dish B and dish B can be 
// similar to dish C, so dish C is a good substitution for dish A even if theu aren't directly
// similar. A path in the graph would represent this relationship, but direct comparison will not.
public sealed class SubstitutionFinder
{
    private readonly int _heapArity;

    public SubstitutionFinder(int heapArity = 4) => _heapArity = heapArity;

    // Find substitution for a dish in slot with slotIndex
    public SubstitutionResult Find(
        PlanningProblem problem,
        SimilarityGraph graph,
        IReadOnlyList<int> assignment,
        int slotIndex,
        IReadOnlySet<int>? excluded = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(problem);
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(assignment);
        ArgumentOutOfRangeException.ThrowIfNegative(slotIndex);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slotIndex, assignment.Count);

        int rejected = assignment[slotIndex];
        SlotDescriptor slot = problem.Spec.Slots[slotIndex];

        var candidate = new int[assignment.Count];

        for (int i = 0; i < assignment.Count; i++)
        {
            candidate[i] = assignment[i];
        }

        var distance = new double[graph.VertexCount];
        var previous = new int[graph.VertexCount];
        var settled = new bool[graph.VertexCount];

        Array.Fill(distance, double.PositiveInfinity);
        Array.Fill(previous, -1);

        var heap = new IndexedDaryHeap<double>(graph.VertexCount, _heapArity);

        distance[rejected] = 0;
        heap.Push(rejected, 0);

        long exploredNodes = 0;

        while (!heap.IsEmpty)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int current = heap.PopMin();
            settled[current] = true;
            exploredNodes++;

            // Vertices are extracted in non-decreasing order of distance, therefore
            // the first acceptable one is the nearest. the rejested dish together
            // with other excluded previously by user dishes are skipped
            if (current != rejected &&
                excluded?.Contains(current) != true &&
                IsAcceptable(problem, candidate, slotIndex, slot, current))
            {
                return new SubstitutionResult(
                    Found: true,
                    current,
                    distance[current],
                    BuildPath(previous, rejected, current),
                    exploredNodes,
                    FailureReason: null);
            }

            ReadOnlySpan<int> neighbors = graph.NeighborsOf(current);
            ReadOnlySpan<double> weights = graph.WeightsOf(current);

            for (int i = 0; i < neighbors.Length; i++)
            {
                int next = neighbors[i];

                if (settled[next])
                {
                    continue;
                }

                // Dijkstra's relaxation. if a shorter path to next is found,
                // its distance and predecessor updates + priority is decreased
                double relaxed = distance[current] + weights[i];

                if (relaxed >= distance[next])
                {
                    continue;
                }

                distance[next] = relaxed;
                previous[next] = current;
                heap.PushOrDecrease(next, relaxed);
            }
        }

        return SubstitutionResult.NotFound(
            "No reachable dish in the similarity graph satisfies all constraints." +
            "Try relaxing the constraints or rejecting fewer dishes.",
            exploredNodes);
    }

    // Decides can the dish be a substitution. It must be valid
    // for the slot and not make the meal plan infeasible.
    private static bool IsAcceptable(
        PlanningProblem problem,
        int[] candidate,
        int slotIndex,
        SlotDescriptor slot,
        int dish)
    {
        // if dish does not belong to the candidate domain of this slot
        if (!problem.Domains.ForSlot(slot).Contains(dish))
        {
            return false;
        }

        // The dish is temprary replaced and the new plan is validated
        int original = candidate[slotIndex];
        candidate[slotIndex] = dish;

        bool feasible = ConstraintValidator.IsFeasible(problem, candidate);

        candidate[slotIndex] = original;

        return feasible;
    }

    // Reconstructs the path from the rejected dish to its replacement.
    private static int[] BuildPath(int[] previous, int from, int to)
    {
        var reversed = new List<int>();
        int node = to;

        while (node >= 0)
        {
            reversed.Add(node);

            if (node == from)
            {
                break;
            }

            node = previous[node];
        }

        reversed.Reverse();

        return [.. reversed];
    }

}
