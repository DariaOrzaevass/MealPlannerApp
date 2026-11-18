using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.Dishes.GetDishes;

// Dish row in the catalog administration list.
public sealed record DishListItemResponse(
    Guid Id,
    string Name,
    decimal Cost,
    string Currency,
    int Calories,
    int CookingTimeMinutes,
    DietType Diet,
    Cuisine Cuisine,
    MealTypes SuitableFor,
    int IngredientCount,
    bool IsPublished);

// Page of dishes
public sealed record DishPageResponse(
    IReadOnlyList<DishListItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);
