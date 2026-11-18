using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Application.Dishes.CreateDish;

// Validates input before adding new dish
internal sealed class CreateDishCommandValidator : AbstractValidator<CreateDishCommand>
{
    public CreateDishCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithErrorCode("Dish.NameRequired")
            .WithMessage("Dish name is required.")
            .MaximumLength(200).WithErrorCode("Dish.NameTooLong")
            .WithMessage("Dish name must not exceed 200 characters.");

        RuleFor(c => c.Recipe)
            .NotEmpty().WithErrorCode("Dish.RecipeRequired")
            .WithMessage("Recipe is required.");

        RuleFor(c => c.Cost)
            .GreaterThan(0).WithErrorCode("Dish.NonPositiveCost")
            .WithMessage("Cost must be greater than zero.");

        RuleFor(c => c.CurrencyCode)
            .Must(code => Currency.TryFromCode(code) is not null)
            .WithErrorCode("Dish.UnknownCurrency")
            .WithMessage("Unknown currency code.");

        RuleFor(c => c.Calories)
            .GreaterThan(0).WithErrorCode("Dish.NonPositiveCalories")
            .WithMessage("Calories must be greater than zero.");

        RuleFor(c => c.CookingTimeMinutes)
            .GreaterThan(0).WithErrorCode("Dish.NonPositiveCookingTime")
            .WithMessage("Cooking time must be greater than zero.");

        RuleFor(c => c.SuitableFor)
            .NotEqual(MealTypes.None).WithErrorCode("Dish.NoMealTypes")
            .WithMessage("Specify at least one meal type.");

        RuleFor(c => c.Diet).IsInEnum().WithErrorCode("Dish.UnknownDiet")
            .WithMessage("Unknown diet type.");

        RuleFor(c => c.Cuisine).IsInEnum().WithErrorCode("Dish.UnknownCuisine")
            .WithMessage("Unknown cuisine.");

        // The ingredients are not required when the dish is created.
        // It becomes required when the dish is published (domain rule).
        // However, if ingredients are provided they must be valid.
        RuleForEach(c => c.Ingredients).ChildRules(ingredient =>
        {
            ingredient.RuleFor(i => i.IngredientId)
                .NotEqual(Guid.Empty).WithErrorCode("Dish.EmptyIngredientId")
                .WithMessage("Ingredient idetirier cannot be empty.");

            ingredient.RuleFor(i => i.Quantity)
                .GreaterThan(0).When(i => i.Unit != MeasurementUnit.ToTaste)
                .WithErrorCode("Dish.NonPositiveQuantity")
                .WithMessage("Quantity must be greater than zero.");

            ingredient.RuleFor(i => i.Unit)
                .IsInEnum().WithErrorCode("Dish.UnknownUnit")
                .WithMessage("Unknown measurement unit.");
        });

        RuleFor(c => c.Ingredients)
            .Must(list => list.Select(i => i.IngredientId).Distinct().Count() == list.Count)
            .WithErrorCode("Dish.DuplicateIngredient")
            .WithMessage("The same ingredient must not appear more than once.");
    }
}
