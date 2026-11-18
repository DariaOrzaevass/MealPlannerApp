using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes;

namespace MealPlanner.Application.Dishes.SetDishPublication;

// Changes the publication state of the dish.
internal sealed class SetDishPublicationCommandHandler(
    IDishRepository dishes,
    IUnitOfWork unitOfWork) : ICommandHandler<SetDishPublicationCommand>
{
    public async Task<Result> Handle(
        SetDishPublicationCommand command,
        CancellationToken cancellationToken)
    {
        var dishId = DishId.From(command.DishId);

        Dish? dish = await dishes.GetByIdAsync(dishId, cancellationToken);

        if (dish is null)
        {
            return Result.Failure(DishErrors.NotFound(dishId));
        }

        if (command.Published)
        {
            // Publishing can fail. Dish without ingredients cannot be published,
            // because it would have empty allergen set and pass all allergen filters.
            Result published = dish.Publish();

            if (published.IsFailure)
            {
                return published;
            }
        }
        else
        {
            dish.Unpublish();
        }

        // Saving dispatches domain events, and their handlers
        // will invalidate the snapshot of the catalog.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
