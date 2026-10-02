using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Shared;

//Currency record for the money 
public sealed record Currency
{
    //Predefined currencies, can be more in the future
    public static readonly Currency None = new("");
    public static readonly Currency Usd = new("USD");
    public static readonly Currency Eur = new("EUR");

    public static IReadOnlyCollection<Currency> All => new[] { Usd, Eur };

    //Returns currency from code and throws exception if encounters unknown code (TryFromCode returns null)
    public static Currency FromCode(string code) =>
       TryFromCode(code)
       ?? throw new ArgumentException($"Unknown currency code: {code}", nameof(code));

    //Returns currency from code or null if has not found the currency
    public static Currency? TryFromCode(string code) =>
        All.FirstOrDefault(c => c.Code == code);

    //Constructor for the currency record, private so noone can create a new currency outside the class
    private Currency(string code) => Code = code;

    //Code of the currency, like EUR for example
    public string Code { get; init; }
}
