using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;

namespace MealPlanner.Application.Dishes.GetDishes;

// Returns a page of dish catalog
internal sealed class GetDishesQueryHandler(IDishReadRepository dishes)
    : IQueryHandler<GetDishesQuery, DishPageResponse>
{
    // Maximum page size. With this requests cannot
    // force the database to return all of the catalog.
    private const int _maxPageSize = 100;

    public async Task<Result<DishPageResponse>> Handle(
    GetDishesQuery query,
    CancellationToken cancellationToken)
    {
        int page = Math.Max(1, query.Page);

        // Page size is never smaller than 1
        int pageSize = Math.Clamp(query.PageSize, 1, _maxPageSize);

        DishPageResponse result = await dishes.GetPageAsync(
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
            query.PublishedOnly,
            page,
            pageSize,
            cancellationToken);

        return result;
    }
}
