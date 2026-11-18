using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.Dishes.GetDishes;

// Reads the dish catalog page by page without loading the aggregate.
public interface IDishReadRepository
{
    Task<DishPageResponse> GetPageAsync(
        string? search,
        bool? publishedOnly,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
