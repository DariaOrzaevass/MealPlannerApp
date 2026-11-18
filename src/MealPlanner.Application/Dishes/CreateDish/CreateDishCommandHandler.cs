using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Application.Abstractions.Messaging;
using MealPlanner.Domain.Abstractions;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.Dishes.CreateDish;

// Creates a dish with its ingredients.
// The existance of ingredients is checked before the dish is constructed:
// Without this check, a reference to a non-existent ingredient would reach
// the database and cause a foreign key constraint violation, resulting in
// an exception instead of a meaningful failure result.
internal sealed class CreateDishCommandHandler(
    IDishRepository dishes,
    IIngredientRepository ingredients,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateDishCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateDishCommand command,
        CancellationToken cancellationToken)
    {
        Currency? currency = Currency.TryFromCode(command.CurrencyCode);

        if (currency is null)
        {
            return Result.Failure<Guid>(DishErrors.UnknownCurrency);
        }

        Result missing = await CheckIngredientsExistAsync(command, cancellationToken);

        if (missing.IsFailure)
        {
            return Result.Failure<Guid>(missing.Error);
        }

        Result<Dish> dish = Dish.Create(
            command.Name,
            command.Description,
            command.Recipe,
            new Money(command.Cost, currency),
            command.Calories,
            command.SuitableFor,
            command.CookingTimeMinutes,
            command.Diet,
            command.Cuisine);

        if (dish.IsFailure)
        {
            return Result.Failure<Guid>(dish.Error);
        }

        foreach (DishIngredientInput ingredient in command.Ingredients)
        {
            Result added = dish.Value.AddIngredient(
                IngredientId.From(ingredient.IngredientId),
                ingredient.Quantity,
                ingredient.Unit);

            if (added.IsFailure)
            {
                return Result.Failure<Guid>(added.Error);
            }
        }

        dishes.Add(dish.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return dish.Value.Id.Value;
    }

    private async Task<Result> CheckIngredientsExistAsync(
        CreateDishCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Ingredients.Count == 0)
        {
            return Result.Success();
        }

        IReadOnlyCollection<Ingredient> all = await ingredients.GetAllAsync(cancellationToken);

        HashSet<Guid> known = [.. all.Select(i => i.Id.Value)];

        return command.Ingredients.All(i => known.Contains(i.IngredientId))
            ? Result.Success()
            : Result.Failure(DishErrors.UnknownIngredient);
    }
}
