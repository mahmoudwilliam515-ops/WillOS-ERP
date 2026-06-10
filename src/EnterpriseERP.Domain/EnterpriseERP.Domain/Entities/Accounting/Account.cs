using EnterpriseERP.SharedKernel.Common;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Domain.Entities.Accounting;

public enum AccountType
{
    Asset,
    Liability,
    Equity,
    Revenue,
    Expense
}

public enum CashFlowCategory
{
    None,
    Operating,
    Investing,
    Financing
}

public enum AccountCategory
{
    AccountsReceivable,
    AccountsPayable,
    Inventory,
    FixedAsset,
    DepreciationExpense,
    AmortizationExpense,
    Revenue,
    Expense,
    Other
}

/// <summary>IFRS balance sheet presentation: current vs non-current.</summary>
public enum BalanceSheetClassification
{
    NotApplicable = 0,
    Current = 1,
    NonCurrent = 2
}

public class Account : AuditableEntity, IAggregateRoot
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public CashFlowCategory CashFlowCategory { get; set; } = CashFlowCategory.None;
    public AccountCategory AccountCategory { get; set; } = AccountCategory.Other;
    public BalanceSheetClassification BalanceSheetClassification { get; set; } = BalanceSheetClassification.NotApplicable;

    public Guid? ParentId { get; set; }
    public Account? Parent { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsLeaf { get; set; } = true;

    public ICollection<Account> SubAccounts { get; set; } = new List<Account>();
}
