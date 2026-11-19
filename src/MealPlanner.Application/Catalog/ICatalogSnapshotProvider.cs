using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Application.Catalog;

// Interface for accessing the catalog snapshot
public interface ICatalogSnapshotProvider
{
    // Returns snapshot of the catalog. Created once on first access and reused later.
    Task<CatalogSnapshot> GetAsync(CancellationToken cancellationToken = default);

    // Invalidates the snapshot. Called when catalog is changed, for example, when a dish is added.
    void Invalidate();
}
