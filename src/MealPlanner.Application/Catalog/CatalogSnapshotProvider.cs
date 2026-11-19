using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Ingredients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Application.Catalog;

// Builds a catalog snapshot and keeps it cached until the catalog is changed.
// Building a snapshot is expensive (reading complete dish catalog, with ingredients
// + similarity tabel O(n^2)), so cannot do this for every user request. The repositories
// stay scoped for individual requests, so separate scope is created explicitely with IServiceScopeFactory.
// They cannot be injected directly into a long-lived service because the database context is not designed for such lifetime.
internal sealed class CatalogSnapshotProvider(
    IServiceScopeFactory scopeFactory,
    ILogger<CatalogSnapshotProvider> logger) : ICatalogSnapshotProvider
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    private CatalogSnapshot? _snapshot;

    public async Task<CatalogSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        // Fast path without locking. the snapshot is read frequently but rebuiled rarely
        CatalogSnapshot? current = _snapshot;

        if (current is not null)
        {
            return current;
        }

        await _gate.WaitAsync(cancellationToken);

        try
        {
            // Check again because anothe request may build the snapshot
            // While this request was waiting. Without this, two
            // concurrent requests could build the same snapshot twice.
            if (_snapshot is not null)
            {
                return _snapshot;
            }

            _snapshot = await BuildAsync(cancellationToken);

            return _snapshot;
        }
        finally
        {
            _gate.Release();
        }
    }

    public void Invalidate()
    {
        _snapshot = null;

        logger.LogInformation("The catalog snapshot has been marked as outdated.");
    }

    private async Task<CatalogSnapshot> BuildAsync(CancellationToken cancellationToken)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();

        IDishRepository dishes = scope.ServiceProvider.GetRequiredService<IDishRepository>();
        IIngredientRepository ingredients =
            scope.ServiceProvider.GetRequiredService<IIngredientRepository>();
        IAllergenRepository allergens =
            scope.ServiceProvider.GetRequiredService<IAllergenRepository>();

        long start = Stopwatch.GetTimestamp();

        CatalogSnapshot snapshot = CatalogSnapshotBuilder.Build(
            await dishes.GetPublishedWithIngredientsAsync(cancellationToken),
            await ingredients.GetAllAsync(cancellationToken),
            await allergens.GetAllAsync(cancellationToken));

        TimeSpan elapsed = Stopwatch.GetElapsedTime(start);

        logger.LogInformation(
            "Catalog snapshot built: {Dishes} dishes, {Ingredients} ingredients, {Elapsed:0} ms.",
            snapshot.DishCount,
            snapshot.Catalog.IngredientCount,
            elapsed.TotalMilliseconds);

        return snapshot;
    }
}
