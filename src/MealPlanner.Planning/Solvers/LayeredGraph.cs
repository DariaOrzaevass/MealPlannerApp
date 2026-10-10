using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Planning.Models;

namespace MealPlanner.Planning.Solvers;

// Class for the Layered Graph for the Dijkstra's algorithm
internal sealed class LayeredGraph
{
    private readonly PlanningProblem _problem;
    private readonly int[] _layerOffsets;
    private readonly int[] _nodeDish;
    private readonly int[] _nodeLayer;

    // Private constructor so that the Layered Graph can only be created inside the class
    private LayeredGraph(PlanningProblem problem,
        int[] layerOffsets,
        int[] nodeDish,
        int[] nodeLayer)
    {
        _problem = problem;
        _layerOffsets = layerOffsets;
        _nodeDish = nodeDish;
        _nodeLayer = nodeLayer;

        DishNodeCount = nodeDish.Length;
        Source = DishNodeCount;
        Sink = DishNodeCount + 1;
        NodeCount = DishNodeCount + 2;
        NodePenalty = new double[DishNodeCount];

    }

    // The number of layers. Is equal to the number of meal plan entries
    public int LayerCount => _layerOffsets.Length - 1;

    // The number of nodes (with source and sink)
    public int NodeCount { get; }

    // The number of nodes assasiated with dishes (source and sink are not included)
    public int DishNodeCount { get; }

    // The source node - no dish here
    public int Source { get; }

    // The finishing node - no dish here
    public int Sink { get; }

    // Which dish does the node corresponds to
    public int DishOf(int node) => _nodeDish[node];

    // At which layer is the node
    public int LayerOf(int node) => _nodeLayer[node];

    // Binary search to find out the node index that corresponds to a
    // particular dish in a particular layer. -1 if dish is not allowed there
    public int NodeOf(int layer, int dish)
    {
        // Indexes that layer occupy from low to high
        int low = _layerOffsets[layer];
        int high = _layerOffsets[layer + 1] - 1;

        while (low <= high)
        {
            int middle = low + ((high - low) / 2);
            int value = _nodeDish[middle];

            if (value == dish)
            {
                return middle;
            }

            if (value < dish)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }

        return -1;
    }

    // An array of node penalties
    public double[] NodePenalty { get; }

    // Exctra multiplier for a cost component in the weight.
    // Can make cost affect algorithm more or less
    public double ExtraCostMultiplier { get; set; }

    // Clears all penalties
    public void ResetPenalties()
    {
        Array.Clear(NodePenalty);
        ExtraCostMultiplier = 0;
    }

    // Returns a range(half-open) of nodes belonging to
    // this layer including the beginning of the next layer
    public (int Start, int End) LayerRange(int layer) =>
    (_layerOffsets[layer], _layerOffsets[layer + 1]);

    // Builds a Layered Graph after validation
    public static LayeredGraph Build(PlanningProblem problem)
    {
        ArgumentNullException.ThrowIfNull(problem);

        return new LayeredGraph(problem, offsets, nodeDish, nodeLayer);
    }
}
