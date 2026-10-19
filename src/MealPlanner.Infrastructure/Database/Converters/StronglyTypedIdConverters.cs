using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Dishes;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.MealPlans;
using MealPlanner.Domain.Users;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MealPlanner.Infrastructure.Database.Converters;

// Converter for the DishId
internal sealed class DishIdConverter() : ValueConverter<DishId, Guid>(
    id => id.Value,
    value => DishId.From(value));

// Converter for the IngredientId
internal sealed class IngredientIdConverter() : ValueConverter<IngredientId, Guid>(
    id => id.Value,
    value => IngredientId.From(value));

// Converter for the AllergenId
internal sealed class AllergenIdConverter() : ValueConverter<AllergenId, Guid>(
    id => id.Value,
    value => AllergenId.From(value));

// Converter for the MealPlanId
internal sealed class MealPlanIdConverter() : ValueConverter<MealPlanId, Guid>(
    id => id.Value,
    value => MealPlanId.From(value));

// Converter for the MealPlanEntryId
internal sealed class MealPlanEntryIdConverter() : ValueConverter<MealPlanEntryId, Guid>(
    id => id.Value,
    value => MealPlanEntryId.From(value));

// Converter for the UserId
internal sealed class UserIdConverter() : ValueConverter<UserId, Guid>(
    id => id.Value,
    value => UserId.From(value));
