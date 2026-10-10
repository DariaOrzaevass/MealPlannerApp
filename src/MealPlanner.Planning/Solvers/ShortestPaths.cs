using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;

namespace MealPlanner.Planning.Solvers;

// Usefull static methods for working with the heap
public static class ShortestPaths
{
    // Weights of paths and the previous vertices
    public static (double[] Distance, int[] Previous) Run(
        (int To, double Weight)[][] edges,
        int source)
    {
        // The number of dishes
        int vertexCount = edges.Length;

        // The best known distance(weight) to each dish
        var distance = new double[vertexCount];

        // From which dish we came from (for the path)
        var previous = new int[vertexCount];

        // Which dish is already popped from the heap
        var settled = new bool[vertexCount];

        // Did not get to any dish yet - so infinity for now
        Array.Fill(distance, double.PositiveInfinity);

        // -1 means that there is no previous
        Array.Fill(previous, -1);

        // The heap for all dishes
        var heap = new IndexedBinaryHeap<double>(vertexCount);

        // We begin at the source (starting node)
        distance[source] = 0;

        // pushing sorce in the heap
        heap.Push(source, 0);

        // While there are dishes we have not worked with yet
        while (!heap.IsEmpty)
        {
            // Getting the one with lowest priority
            int current = heap.PopMin();

            // we are not going to change its priority anymore
            settled[current] = true;

            // looking at every path from this dish
            foreach ((int to, double weight) in edges[current])
            {
                // Looking whether the path has become shorter through this dish
                Relax(current, to, weight, distance, previous, settled, heap);
            }
        }

        return (distance, previous);
    }

    // If the new path through the new dish is shorter - we remember it
    private static void Relax(
    int from,
    int to,
    double weight,
    double[] distance,
    int[] previous,
    bool[] settled,
    IndexedBinaryHeap<double> heap)
    {
        // The dish is already not in the neap, cannot become better
        if (settled[to])
        {
            return;
        }

        // The cost of path until "from" dish plus the cost of the path to the new dish
        double candidate = distance[from] + weight;

        // The new path is not shorter than the previous one. Nothing changes
        if (candidate >= distance[to])
        {
            return;
        }

        // Otherwise remember the best cost of the path
        distance[to] = candidate;

        // Remember from which dish we got to the new dish
        previous[to] = from;

        // Push the dish in the heap if it is not there yet or decrease its key
        heap.PushOrDecrease(to, candidate);
    }
}
