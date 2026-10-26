using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Planning.Models;

// Describes one testing dish with data required for dish catalog
public sealed record DishDescriptor(
    int CostMinorUnits,
    int Calories,
    MealKinds SuitableFor,
    int CookingTimeMinutes,
    int DietLevel,
    int CuisineId,
    IReadOnlyList<int> IngredientIds,
    IReadOnlyList<int> AllergenPositions);
