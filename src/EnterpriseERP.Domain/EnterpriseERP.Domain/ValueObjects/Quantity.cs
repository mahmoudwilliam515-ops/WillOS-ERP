using System;
using System.Collections.Generic;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.ValueObjects;

public class Quantity : ValueObject
{
    public decimal Value { get; private set; }

    private Quantity(decimal value)
    {
        Value = value;
    }

    public static Quantity Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(value));

        return new Quantity(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
