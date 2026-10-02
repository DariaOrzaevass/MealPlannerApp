using System;
using System.Collections.Generic;
using System.Text;

namespace MealPlanner.Domain.Shared;

//Money record with amount of money and currency for prices of meals and ingredients
public record Money(decimal Amount, Currency Currency)
{
    //Create zero money with or without specific currency and check for zero money
    public static Money Zero() => new(0, Currency.None);
    public static Money Zero(Currency currency) => new(0, currency);
    public bool IsZero() => this == Zero(Currency);

    //Overloads + operator that does not allow adding money in different currencies
    public static Money operator +(Money first, Money second)
    {
        if (first.IsZero())
        {
            return new Money(second.Amount, second.Currency);
        }

        if (second.IsZero())
        {
            return new Money(first.Amount, first.Currency);
        }

        return first.Currency != second.Currency ?
            throw new InvalidOperationException("Cannot add money in different currencies") :
            new Money(first.Amount + second.Amount, first.Currency);
    }

    //Overloads - operator that does not allow subtracting money in different currencies
    public static Money operator -(Money first, Money second)
    {
        if (second.IsZero())
        {
            return new Money(first.Amount, first.Currency);
        }

        if (!first.IsZero() && first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Cannot subtract money in different currencies");
        }

        return new Money(first.Amount - second.Amount, first.IsZero() ? second.Currency : first.Currency);
    }

    //Overrides ToString method to return the amount of money and currency code to make it readable, like "5.00 EUR"
    public override string ToString() => $"{Amount:0.##} {Currency.Code}";
}
