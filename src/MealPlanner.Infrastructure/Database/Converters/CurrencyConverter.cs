using System;
using System.Collections.Generic;
using System.Text;
using MealPlanner.Domain.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MealPlanner.Infrastructure.Database.Converters;

// Converter for the Currency to string and back for the Entity Framework Core
internal sealed class CurrencyConverter() : ValueConverter<Currency, string>(
    currency => currency.Code,
    code => Currency.FromCode(code));
