using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Allergens;
using MealPlanner.Domain.Users;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MealPlanner.Infrastructure.Database.Converters;


// Converter for the AllergenId
internal sealed class AllergenIdConverter() : ValueConverter<AllergenId, Guid>(
    id => id.Value,
    value => AllergenId.From(value));

// Converter for the UserId
internal sealed class UserIdConverter() : ValueConverter<UserId, Guid>(
    id => id.Value,
    value => UserId.From(value));
