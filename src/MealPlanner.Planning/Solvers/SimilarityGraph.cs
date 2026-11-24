using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.DataStructures;
using MealPlanner.Planning.Models;
using MealPlanner.Planning.Scoring;

namespace MealPlanner.Planning.Solvers;

// Similarity graph where vertices represent dishes, edges connect similar enough dishes, and
// edge represents distance between them. This graph is needed for when the user rejects a dish
// and wants to replace it. The application then looks for the nearest valid alernative. Finding
// nearest vertex in a graph with non-negative edge weight can be solved with Dijkstra's again.
// Unlike layered graph of the weekly plan this one has cycles.
public sealed class SimilarityGraph
{
    private readonly int[] _offsets;
    private readonly int[] _neighbors;
    private readonly double[] _weights;

    private SimilarityGraph(int[] offsets, int[] neighbors, double[] weights, double threshold)
    {
        _offsets = offsets;
        _neighbors = neighbors;
        _weights = weights;
        Threshold = threshold;
    }

    // How many dishes there are in graph
    public int VertexCount => _offsets.Length - 1;

    // How many directed edges there are in a graph. the grapf is undirected, so this is twice the number of connected dish pairs.
    public int EdgeCount => _neighbors.Length;

    // Similarity threshold below which the edge is not created
    public double Threshold { get; }

    // Returns neighbours of the dish
    public ReadOnlySpan<int> NeighborsOf(int dish)
    {
        int start = _offsets[dish];

        return _neighbors.AsSpan(start, _offsets[dish + 1] - start);
    }

    // Returns edge weights for neighbours in the same order as NeighborsOf
    public ReadOnlySpan<double> WeightsOf(int dish)
    {
        int start = _offsets[dish];

        return _weights.AsSpan(start, _offsets[dish + 1] - start);
    }

    // Builds the similarity graph.The higher the similarity threshold,
    // the sparser the graph is and the faster the search, but many fitting
    // replacements can become unreachable. Similarity metric does not
    // consider price, but price is important for choosing the replacement,
    // replacing a cheap dish with a very expensive is a bad idea.
    public static SimilarityGraph Build(
        DishCatalog catalog,
        DishIndex index,
        SimilarityMetric similarity,
        double threshold = 0.25,
        double costWeight = 0.5)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(index);
        ArgumentNullException.ThrowIfNull(similarity);

        int dishCount = catalog.Count;
        int maxCost = Math.Max(1, catalog.Count == 0 ? 1 : catalog.CostMinorUnits.Max());

        var adjacency = new List<(int Neighbor, double Weight)>[dishCount];

        for (int dish = 0; dish < dishCount; dish++)
        {
            adjacency[dish] = [];
        }

        var candidates = new List<int>();
        var seen = new int[dishCount];
        Array.Fill(seen, -1);

        // Candidate neighbours are also selected by cuisine. Without it
        // pairs with same cuisine, but no similar ingredients will be missed.
        List<int>[] byCuisine = GroupByCuisine(catalog);

        for (int dish = 0; dish < dishCount; dish++)
        {
            CollectCandidates(catalog, index, byCuisine, dish, candidates, seen);

            foreach (int other in candidates)
            {
                // Each pair is considered once, graph is undirected
                if (other <= dish)
                {
                    continue;
                }

                double closeness = similarity.Between(dish, other);

                if (closeness < threshold)
                {
                    continue;
                }

                double weight = (1.0 - closeness)
                    + (costWeight
                        * Math.Abs(catalog.CostMinorUnits[dish] - catalog.CostMinorUnits[other])
                        / maxCost);

                adjacency[dish].Add((other, weight));
                adjacency[other].Add((dish, weight));
            }
        }

        return Pack(adjacency, threshold);
    }

    // Collects candidates whose similarity with this dish can be exceed th threshold.
    // If two dishes have no common ingrediets, not same cuisine their similarity cannot
    // exceed 0.1. With threshold greater than 0.1 such pairs are not even considered.
    // Candidate dishes obtained with inverted index + list of dishes with same cuisine.
    private static void CollectCandidates(
        DishCatalog catalog,
        DishIndex index,
        List<int>[] byCuisine,
        int dish,
        List<int> candidates,
        int[] seen)
    {
        candidates.Clear();

        foreach (int ingredient in catalog.IngredientsOf(dish))
        {
            foreach (int other in index.DishesWith(ingredient))
            {
                // A marker "already added for this dish" is used instead of
                // clearing the array, like with building inverted index.
                if (seen[other] == dish)
                {
                    continue;
                }

                seen[other] = dish;
                candidates.Add(other);
            }
        }

        foreach (int other in byCuisine[catalog.CuisineIds[dish]])
        {
            if (seen[other] == dish)
            {
                continue;
            }

            seen[other] = dish;
            candidates.Add(other);
        }
    }

    // Groups dishes by cuisine
    private static List<int>[] GroupByCuisine(DishCatalog catalog)
    {
        int maxCuisine = 0;

        for (int dish = 0; dish < catalog.Count; dish++)
        {
            maxCuisine = Math.Max(maxCuisine, catalog.CuisineIds[dish]);
        }

        var byCuisine = new List<int>[maxCuisine + 1];

        for (int i = 0; i < byCuisine.Length; i++)
        {
            byCuisine[i] = [];
        }

        for (int dish = 0; dish < catalog.Count; dish++)
        {
            byCuisine[catalog.CuisineIds[dish]].Add(dish);
        }

        return byCuisine;
    }

    // Convert the graph into arrays for the Compressed Sparse Row representation
    private static SimilarityGraph Pack(List<(int Neighbor, double Weight)>[] adjacency, double threshold)
    {
        var offsets = new int[adjacency.Length + 1];

        for (int dish = 0; dish < adjacency.Length; dish++)
        {
            offsets[dish + 1] = offsets[dish] + adjacency[dish].Count;
        }

        var neighbors = new int[offsets[^1]];
        var weights = new double[offsets[^1]];

        for (int dish = 0; dish < adjacency.Length; dish++)
        {
            int write = offsets[dish];

            // Neighbours are sorted by the dish index. So when
            // distances are equal. Dijkstra selects the smaller index.
            adjacency[dish].Sort((left, right) => left.Neighbor.CompareTo(right.Neighbor));

            foreach ((int neighbor, double weight) in adjacency[dish])
            {
                neighbors[write] = neighbor;
                weights[write] = weight;
                write++;
            }
        }

        return new SimilarityGraph(offsets, neighbors, weights, threshold);
    }
}
