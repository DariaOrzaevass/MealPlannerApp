using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Dishes.Events;
using Microsoft.Extensions.Logging;

namespace MealPlanner.Application.Catalog.Invalidation;

// Invalidates the catalog snapshot when it is changed. The snapshot is invalidated and not partially
// updated because it has structure like inverted index, similarity table, and similarity graph.
// Adding/removing even one dish affects all of them and requires recalculation - essentially repeating rebuilding.
internal static class CatalogChangeHandlers
{
    internal sealed class DishPublished(
        ICatalogSnapshotProvider snapshots,
        ILogger<DishPublished> logger)
        : IDomainEventHandler<DishPublishedDomainEvent>
    {
        public Task Handle(DishPublishedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Dish {DishId} was published - the catalog snapshot is outdated now.",
                domainEvent.DishId.Value);

            snapshots.Invalidate();

            return Task.CompletedTask;
        }
    }

    internal sealed class DishUnpublished(
        ICatalogSnapshotProvider snapshots,
        ILogger<DishUnpublished> logger)
        : IDomainEventHandler<DishUnpublishedDomainEvent>
    {
        public Task Handle(
            DishUnpublishedDomainEvent domainEvent,
            CancellationToken cancellationToken)
        {
            logger.LogInformation(
                "Dish {DishId} was unpublished - the catalog snapshot is outdated now.",
                domainEvent.DishId.Value);

            snapshots.Invalidate();

            return Task.CompletedTask;
        }
    }
}
