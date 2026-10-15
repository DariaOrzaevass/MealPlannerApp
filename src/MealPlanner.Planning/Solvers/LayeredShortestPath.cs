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
    /*public static Result Run(
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

            // 
            RelaxOutgoing(graph, current, distance, previous, settled, heap);
        }

        // The heap stayed in positive infinity state
        if (double.IsPositiveInfinity(distance[graph.Sink]))
        {
            return new Result(Found: false, [], double.PositiveInfinity, exploredNodes);
        }

        //
        return new Result(
            Found: true,
            Reconstruct(graph, previous),
            distance[graph.Sink],
            exploredNodes);
    }
    
    private static void RelaxOutgoing(
        LayeredGraph graph,
        int current,
        double[] distance,
        int[] previous,
        bool[] settled,
        IndexedBinaryHeap<double> heap){}
    private static int[] Reconstruct(LayeredGraph graph, int[] previous){}*/
}
