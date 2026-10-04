using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Domain.Dishes;

// A dish entity representing a specific dish
public sealed class Dish : Entity<DishId>
{
    // The list of ingredients of the dish
    private readonly List<DishIngredient> _ingredients = [];

    // Default constructor for EF core to get the dish from the database
    private Dish()
    {
    }

    // The name of the dish to display to the user
    public string Name { get; set; } = null!;
}
