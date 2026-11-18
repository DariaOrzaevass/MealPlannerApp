using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Dishes.GetDishes;
using MealPlanner.Domain.Dishes;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Reads the dish catalog page by page using projection
internal sealed class DishReadRepository(ApplicationDbContext context) : IDishReadRepository
{
    public async Task<DishPageResponse> GetPageAsync(
        string? search,
        bool? publishedOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Dish> query = context.Set<Dish>();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(d => d.Name.Contains(search));
        }

        if (publishedOnly is not null)
        {
            query = query.Where(d => d.IsPublished == publishedOnly);
        }

        // Counting and retrieving the page are two queries. In SQL Server
        // they can be done with one query using a window function over the
        // result set, but using two queries is easier and makes it more readable
        int total = await query.CountAsync(cancellationToken);

        List<DishListItemResponse> items = await query
            .OrderBy(d => d.Name)
            .ThenBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DishListItemResponse(
                d.Id.Value,
                d.Name,
                d.Cost.Amount,
                d.Cost.Currency.Code,
                d.Calories,
                d.CookingTimeMinutes,
                d.Diet,
                d.Cuisine,
                d.SuitableFor,
                d.Ingredients.Count,
                d.IsPublished))
            .ToListAsync(cancellationToken);

        return new DishPageResponse(items, total, page, pageSize);
    }
}
