using MealPlanner.Planning.Models;
using MealPlanner.Planning.Solvers;

namespace MealPlanner.Planning.Tests;

// Tests ShortestPaths against Bellman-Ford
public sealed class ShortestPathsTests
{
    // Tests that ShortestPaths.Run() returns same shortest
    // distances as Bellman-Ford for same small random graphs
    [Fact]
    public void Matches_Bellman_Ford_on_small_random_graphs()
    {
        // Loop generates 20 random graphs with same seeds so
        // same graphs are generated every time the test runs
        for (int seed = 1; seed <= 20; seed++)
        {
            // Generates graph with 8 vertices
            (int To, double Weight)[][] edges = RandomGraph(vertexCount: 8, seed);

            // Runs Bellman-Ford, only cares about the distances
            (double[] expected, _) = BellmanFord(edges, source: 0);

            // Runs ShortestPaths, only cares about the distances
            (double[] actual, _) = ShortestPaths.Run(edges, source: 0);

            // Checks that returned number of distances are equal
            Assert.Equal(expected.Length, actual.Length);

            // Compares double values of shortest distance
            // for every vertex to 9 decimal places
            for (int vertex = 0; vertex < expected.Length; vertex++)
            {
                Assert.Equal(expected[vertex], actual[vertex], precision: 9);
            }
        }
    }

    // Checks that ShortestPaths.Run() will throw an exception at negative weight edge
    [Fact]
    public void Rejects_negative_weight()
    {
        // Creates a graph with negative weight edge from v0 to v1
        (int To, double Weight)[][] edges = [[(1, -0.7)], []];

        Assert.Throws<ArgumentOutOfRangeException>(() => ShortestPaths.Run(edges, 0));
    }

    // Checks that ShortestPaths.Run() will throw an exception at NaN weight edge
    [Fact]
    public void Rejects_nan_weight()
    {
        // Creates a graph with NaN weight edge from v0 to v1
        (int To, double Weight)[][] edges = [[(1, double.NaN)], []];

        Assert.Throws<ArgumentOutOfRangeException>(() => ShortestPaths.Run(edges, 0));
    }

    // Checks that LayeredGraphDijkstraSolver will reject the negative dish cost
    [Fact]
    public void Layered_search_rejects_negative_dish_cost()
    {
        // Creates a dish with negative cost
        DishCatalog catalog = CatalogSamples.Create(
            new CatalogSamples.Draft(-1, 100, MealKinds.Breakfast, []));

        // Creates simple Planning Specification
        PlanningSpec spec = PlanningSpec.Create(10_000, 1, [MealKinds.Breakfast]);

        // Creates complete Planning Problem
        PlanningProblem problem = PlanningProblem.Create(catalog, spec);

        // checks that exception is thrown, when the dish cost is negative
        Assert.Throws<ArgumentOutOfRangeException>(() => new LayeredGraphDijkstraSolver().Solve(problem));
    }

    // Helper that creates a random graph
    private static (int To, double Weight)[][] RandomGraph(int vertexCount, int seed)
    {
        var random = new Random(seed);
        var edges = new List<(int To, double Weight)>[vertexCount];

        // Loops through all vertices
        for (int from = 0; from < vertexCount; from++)
        {
            // Creates empty list of outgoing edges
            edges[from] = [];

            for (int to = 0; to < vertexCount; to++)
            {
                // Prevents self-loops and the graph is not 100% connected
                if (from == to || random.NextDouble() > 0.45)
                {
                    continue;
                }

                // Adds edges with random weights from 0 to 20
                edges[from].Add((to, random.Next(0, 21)));
            }
        }

        // Returns graph in (int To, double Weight)[][] format
        return [.. edges.Select(static list => list.ToArray())];
    }

    // Reference implementation of Bellman-Ford
    private static (double[] Distance, int[] Previous) BellmanFord(
        (int To, double Weight)[][] edges,
        int source)
    {
        // Number of vertices
        int vertexCount = edges.Length;

        // Creates standard shortest-path arrays and fill them
        var distance = new double[vertexCount];
        var previous = new int[vertexCount];
        Array.Fill(distance, double.PositiveInfinity);
        Array.Fill(previous, -1);
        distance[source] = 0;

        // Runs Bellman-Ford relaxation passes.
        for (int iteration = 0; iteration < vertexCount - 1; iteration++)
        {
            // Remembers if anything was changed(improved) during this iteration
            bool changed = false;

            for (int from = 0; from < vertexCount; from++)
            {
                // Skips if vertex is not reached
                if (double.IsPositiveInfinity(distance[from]))
                {
                    continue;
                }

                // Relaxes outgoing edges of from
                foreach ((int to, double weight) in edges[from])
                {
                    // Calculates reaching to through from
                    double candidate = distance[from] + weight;

                    // If new path is better updates the
                    // shortest known distance and previous
                    if (candidate < distance[to])
                    {
                        distance[to] = candidate;
                        previous[to] = from;
                        changed = true;
                    }
                }
            }

            // Breaks if there is no
            // improvement after the pass
            if (!changed)
            {
                break;
            }
        }

        return (distance, previous);
    }
}
