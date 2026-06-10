using System;
using System.Collections.Generic;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.ValueObjects;

public class Currency : ValueObject
{
    public string Code { get; private set; }

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Currency code must be exactly 3 characters.", nameof(code));

        return new Currency(code.ToUpperInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
