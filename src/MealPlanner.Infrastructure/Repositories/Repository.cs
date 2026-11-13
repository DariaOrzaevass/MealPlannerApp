using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Infrastructure.Repositories;

// Basic abstract repository
internal abstract class Repository<TEntity, TEntityId>(ApplicationDbContext context)
    where TEntity : Entity<TEntityId>
    where TEntityId : notnull
{
    protected ApplicationDbContext Context { get; } = context;

    public virtual async Task<TEntity?> GetByIdAsync(
        TEntityId id,
        CancellationToken cancellationToken = default) =>
        await Context.Set<TEntity>()
            .FirstOrDefaultAsync(entity => entity.Id.Equals(id), cancellationToken);

    public void Add(TEntity entity) => Context.Add(entity);

    public void Remove(TEntity entity) => Context.Remove(entity);
}
