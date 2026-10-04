using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Ingredients;

// The product category of the ingredient for regenerating dish with similar ingredients
public enum IngredientCategory
{
    Other = 0,
    Vegetable = 1,
    Fruit = 2,
    Grain = 3,
    Meat = 4,
    Fish = 5,
    Dairy = 6,
    Egg = 7,
    Legume = 8,
    Nut = 9,
    Spice = 10,
    Oil = 11,
    Sweetener = 12
}
