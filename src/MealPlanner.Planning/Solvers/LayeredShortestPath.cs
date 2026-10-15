using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Solvers;

// Dijkstra's search for the shortest path in the layered graph 
internal static class LayeredShortestPath
{
    // The result of the search. Found - is there a path. Assignment - dishes in slots/layers.
    // PathWeight - the weight of the path with penalties. ExploredNodes - number of explored nodes.
    internal readonly record struct Result(
        bool Found,
        int[] Assignment,
        double PathWeight,
        long ExploredNodes);

    // The main Dijkstra method
    public static Result Run(
        LayeredGraph graph,
        int heapArity,
        CancellationToken cancellationToken)
    {
        // The cheapest path from the source to this node (g(n))
        var distance = new double[graph.NodeCount];

        // The previous node that comes before this one
        var previous = new int[graph.NodeCount];

        // Has Dijksta settled on this node
        var settled = new bool[graph.NodeCount];

        // While no path the distance is not known so positive infinity.
        Array.Fill(distance, double.PositiveInfinity);

        // In the beginning no previous nodes - all are -1
        Array.Fill(previous, -1);

        // Creates new Indexed Binary Heap
        var heap = new IndexedBinaryHeap<double>(graph.NodeCount, heapArity);

        // Source has 0 distance - it is the beginning of the search
        distance[graph.Source] = 0;
        heap.Push(graph.Source, 0);

        // No nodes have been explored yet
        long exploredNodes = 0;

        // Main Dijkstra's algorithm loop
        while (!heap.IsEmpty)
        {
            // Allows to stop the algorithm from outside
            cancellationToken.ThrowIfCancellationRequested();

            // Gets the node with minimum distance (not settled for now)
            int current = heap.PopMin();

            // Settle the node since its distance is finalized now
            // The longer path cannot be better than the settled one
            // Dijkstra require non-negative weights
            settled[current] = true;
            exploredNodes++;

            // Reached the sink. No need to continue.
            // No later nodes are cheaper in reaching the sink
            if (current == graph.Sink)
            {
                break;
            }

            // Edge relaxation for all nodes reachable from the current
            RelaxOutgoing(graph, current, distance, previous, settled, heap);
        }

        // The heap stayed in positive infinity state(couldn't reach the end of the meal plan)
        if (double.IsPositiveInfinity(distance[graph.Sink]))
        {
            return new Result(Found: false, [], double.PositiveInfinity, exploredNodes);
        }

        // The sink was reached successfully
        return new Result(
            Found: true,
            Reconstruct(graph, previous),
            distance[graph.Sink],
            exploredNodes);
    }

    // Relaxes all edges from the current node depending on where it is. 
    private static void RelaxOutgoing(
        LayeredGraph graph,
        int current,
        double[] distance,
        int[] previous,
        bool[] settled,
        IndexedBinaryHeap<double> heap)
    {
        // If the current node is the source - the beginning
        if (current == graph.Source)
        {
            // Source is connected to every dish in the first layer - layer 0
            (int start, int end) = graph.LayerRange(0);

            for (int next = start; next < end; next++)
            {
                // The weight is entry weigh since there are no previous dishes
                Relax(current, next, graph.EntryWeight(next), distance, previous, settled, heap);
            }

            return;
        }

        int layer = graph.LayerOf(current);

        // The last meal slot
        if (layer == graph.LayerCount - 1)
        {
            // The next node will be the sink with no weight - all dishes are already chosen
            Relax(current, graph.Sink, 0, distance, previous, settled, heap);

            return;
        }

        // For nods in other layers
        (int nextStart, int nextEnd) = graph.LayerRange(layer + 1);

        for (int next = nextStart; next < nextEnd; next++)
        {
            // Removes invalid transitions
            if (!graph.IsTransitionAllowed(current, next))
            {
                continue;
            }

            Relax(current, next, graph.TransitionWeight(current, next), distance, previous, settled, heap);
        }
    }

    // Edge relaxation
    private static void Relax(
        int from,
        int to,
        double weight,
        double[] distance,
        int[] previous,
        bool[] settled,
        IndexedBinaryHeap<double> heap)
    {
        // Settled nodes cannot be changed anymore
        if (settled[to])
        {
            return;
        }

        // Calculates the cost of reaching to through from.
        // The distance before is summarized with the weight of this node
        double candidate = distance[from] + weight;

        // The new path is not better than already existing, so do nothing
        if (candidate >= distance[to])
        {
            return;
        }

        // Otherwise records new shortest distance
        distance[to] = candidate;

        // Also record fom where the path is better
        previous[to] = from;

        // Since the node can already be in heap with
        // worse distance we might need the Decrease Key
        heap.PushOrDecrease(to, candidate);
    }

    // Reconstructs the shortest path
    private static int[] Reconstruct(LayeredGraph graph, int[] previous)
    {
        // Creates one dish position for every layer
        var assignment = new int[graph.LayerCount];

        // Sink is not a dish. The first one is right after it
        int node = previous[graph.Sink];

        // For each node puts the correct dish in the meal slot.
        // Goes backwards and constructs the plan in the correct order
        while (node >= 0 && node != graph.Source)
        {
            assignment[graph.LayerOf(node)] = graph.DishOf(node);
            node = previous[node];
        }

        return assignment;
    }
}
