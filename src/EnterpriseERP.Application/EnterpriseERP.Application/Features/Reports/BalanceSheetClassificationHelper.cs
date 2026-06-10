using EnterpriseERP.Domain.Entities.Accounting;

namespace EnterpriseERP.Application.Features.Reports;

public static class BalanceSheetClassificationHelper
{
    public static BalanceSheetClassification Resolve(Account account)
    {
        if (account.BalanceSheetClassification != BalanceSheetClassification.NotApplicable)
            return account.BalanceSheetClassification;

        if (account.Type == AccountType.Equity)
            return BalanceSheetClassification.NotApplicable;

        var code = account.Code.Trim();

        if (account.Type == AccountType.Asset)
        {
            if (code.StartsWith("11", StringComparison.Ordinal))
                return BalanceSheetClassification.Current;

            if (code.StartsWith("12", StringComparison.Ordinal) ||
                code.StartsWith("13", StringComparison.Ordinal) ||
                code.StartsWith("14", StringComparison.Ordinal))
                return BalanceSheetClassification.NonCurrent;

            if (ContainsAny(account.Name, "نقد", "بنك", "عملاء", "مدين", "مخزون", "cash", "bank", "receivable", "inventory"))
                return BalanceSheetClassification.Current;

            return BalanceSheetClassification.NonCurrent;
        }

        if (account.Type == AccountType.Liability)
        {
            if (code.StartsWith("21", StringComparison.Ordinal))
                return BalanceSheetClassification.Current;

            if (code.StartsWith("22", StringComparison.Ordinal) ||
                code.StartsWith("23", StringComparison.Ordinal))
                return BalanceSheetClassification.NonCurrent;

            if (ContainsAny(account.Name, "مورد", "دائن", "قصير", "payable", "short"))
                return BalanceSheetClassification.Current;

            return BalanceSheetClassification.NonCurrent;
        }

        return BalanceSheetClassification.NotApplicable;
    }

    private static bool ContainsAny(string text, params string[] tokens) =>
        tokens.Any(t => text.Contains(t, StringComparison.OrdinalIgnoreCase));
}
