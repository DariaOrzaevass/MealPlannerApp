using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;

namespace MealPlanner.Application.Dishes.GetDishes;

// Returns a page of dishes to the administrator.
// Search - for searching the dish name, if empty - no filtering
public sealed record GetDishesQuery(
    string? Search = null,
    bool? PublishedOnly = null,
    int Page = 1,
    int PageSize = 20) : IQuery<DishPageResponse>;
