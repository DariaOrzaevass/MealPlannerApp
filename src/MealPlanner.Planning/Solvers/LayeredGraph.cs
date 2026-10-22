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

        // Number of layers = Number of slots
        int layerCount = problem.SlotCount;

        // Where each layer begin
        var offsets = new int[layerCount + 1];

        // Builds offsets
        for (int layer = 0; layer < layerCount; layer++)
        {
            int domainSize = problem.Domains.ForSlot(problem.Spec.Slots[layer]).Length;
            offsets[layer + 1] = offsets[layer] + domainSize;
        }

        // Creates two arrays with one entry per node of the graph
        // nodeDish for the dish, nodeLayer for the layer(slot) of it 
        var nodeDish = new int[offsets[layerCount]];
        var nodeLayer = new int[offsets[layerCount]];

        // Fills the node representing arrays
        for (int layer = 0; layer < layerCount; layer++)
        {
            ReadOnlySpan<int> domain = problem.Domains.ForSlot(problem.Spec.Slots[layer]);
            int write = offsets[layer];

            foreach (int dish in domain)
            {
                nodeDish[write] = dish;
                nodeLayer[write] = layer;
                write++;
            }
        }

        return new LayeredGraph(problem, offsets, nodeDish, nodeLayer);
    }

    // Calculates the cost of choosing node in the first layer
    public double EntryWeight(int node)
    {
        int dish = _nodeDish[node];

        // Returns the cost penalty together with preference penalty and node penalty
        return CostTerm(dish) + PreferenceTerm(dish) + NodePenalty[node];
    }

    // The weight of transitioning from one layer to another. Monotony is
    // added here because now we have at least one dish to compare to
    public double TransitionWeight(int fromNode, int toNode)
    {
        int fromDish = _nodeDish[fromNode];
        int toDish = _nodeDish[toNode];

        return CostTerm(toDish)
            + PreferenceTerm(toDish)
            + MonotonyTerm(fromDish, toDish)
            + NodePenalty[toNode];
    }

    // Checks whether transition is allowed
    public bool IsTransitionAllowed(int fromNode, int toNode)
    {
        // If the user hasn't used days before repetition of the dish
        if (_problem.Spec.MinDaysBetweenRepeats <= 0)
        {
            return true;
        }

        // If two different dishes - no problem with repetition
        if (_nodeDish[fromNode] != _nodeDish[toNode])
        {
            return true;
        }

        // Gets days where the dishes are located in the meal plan
        int fromDay = _problem.Spec.Slots[_nodeLayer[fromNode]].Day;
        int toDay = _problem.Spec.Slots[_nodeLayer[toNode]].Day;

        // Returns whether distance in days between dishes are fitting the minimum allowed
        return Math.Abs(toDay - fromDay) >= _problem.Spec.MinDaysBetweenRepeats;
    }

    // Turns the price of the dish to cost penalty
    private double CostTerm(int dish) =>
    (_problem.Weights.Cost + ExtraCostMultiplier)
    * _problem.Catalog.CostMinorUnits[dish]
    / _problem.Spec.BudgetMinorUnits;

    // Defines the penalty of not using the cuisine preffered by user
    private double PreferenceTerm(int dish) =>
        _problem.Weights.Preference * _problem.PreferencePenalty(dish) / LayerCount;

    // Defines the penalty for consecutive similar dishes
    private double MonotonyTerm(int fromDish, int toDish) =>
        LayerCount <= 1
            ? 0.0
            : _problem.Weights.Monotony * _problem.Similarity.Between(fromDish, toDish) / (LayerCount - 1);
}
