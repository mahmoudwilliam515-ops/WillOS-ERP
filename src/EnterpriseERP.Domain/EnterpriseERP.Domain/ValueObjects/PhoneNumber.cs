using System;
using System.Collections.Generic;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    public string Value { get; private set; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number cannot be empty.", nameof(value));

        if (value.Length < 7 || value.Length > 20)
            throw new ArgumentException("Phone number length is invalid.", nameof(value));

        return new PhoneNumber(value);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
