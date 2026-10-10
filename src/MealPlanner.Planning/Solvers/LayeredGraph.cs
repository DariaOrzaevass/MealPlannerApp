using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Solvers;

// Class for the Layered Graph for the Dijkstra's algorithm
internal sealed class LayeredGraph
{
    // Private constructor so that the Layered Graph can only be created inside the class
    private LayeredGraph()
    {

    }

    // The number of nodes (with source and sink)
    public int NodeCount { get; }

    // The number of nodes assasiated with dishes (source and sink are not included)
    public int DishNodeCount { get; }

    // The source node - no dish here
    public int Source { get; }

    // The finishing node - no dish here
    public int Sink { get; }
}
