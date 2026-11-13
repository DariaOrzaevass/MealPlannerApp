using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Database;

// The main EF Core database context that connects the application to the database
public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IDomainEventDispatcher domainEvents) : DbContext(options), IUnitOfWork
{
    // Overrides OnModelCreating() method to build the database
    // model from Configurations in the MealPlanner.Infrastructure
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Calls the base Db.Context model configuration
        base.OnModelCreating(modelBuilder);
    }

    // Overrides SaveChangesAsync() mehtod to add domain-event handling and concurrency handling
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<IDomainEvent> events = CollectDomainEvents();

        int affected;

        try
        {
            affected = await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // A conflict over the same row in database is an expected situation when two users
            // work with the data at the same time. It is not an infrastructure failure.
            // The Application layer should be able to handle it, so the EF Core exception
            // is replaced with its own exception that does not depend on the storage mechanism.
            throw new ConcurrencyException(
                "The data has changed since it was read. Repeat the operation.",
                exception);
        }

        await domainEvents.DispatchAsync(events, cancellationToken);

        return affected;
    }

    // Collects all events that were produced by tracked entities.
    // Clear is mandatory - without it the event will stay in the entity.
    private IReadOnlyCollection<IDomainEvent> CollectDomainEvents()
    {
        List<IEntity> entities =
        [
            .. ChangeTracker.Entries<IEntity>()
                .Select(entry => entry.Entity)
                .Where(entity => entity.GetDomainEvents().Count > 0)
        ];

        List<IDomainEvent> events = [.. entities.SelectMany(entity => entity.GetDomainEvents())];

        foreach (IEntity entity in entities)
        {
            entity.ClearDomainEvents();
        }

        return events;
    }
}

// Custom Concurrency Exception
public sealed class ConcurrencyException(string message, Exception innerException)
    : Exception(message, innerException);
