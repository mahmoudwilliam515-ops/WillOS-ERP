using System;
using System.Collections.Generic;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.ValueObjects;

public class TaxNumber : ValueObject
{
    public string Value { get; private set; }

    private TaxNumber(string value)
    {
        Value = value;
    }

    public static TaxNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Tax number cannot be empty.", nameof(value));

        return new TaxNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
