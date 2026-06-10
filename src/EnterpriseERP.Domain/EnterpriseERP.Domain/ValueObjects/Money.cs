using System;
using System.Collections.Generic;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public Currency Currency { get; private set; }

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, Currency currency)
    {
        if (currency == null)
            throw new ArgumentNullException(nameof(currency));

        return new Money(amount, currency);
    }
    
    public static Money Zero(Currency currency) => new Money(0m, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
