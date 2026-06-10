using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Migration;

/// <summary>
/// Tracks every Opening Balance import session to prevent duplicate imports
/// and maintain a full audit trail of migration events.
/// </summary>
public enum OpeningBalanceStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public class OpeningBalanceSession : AuditableEntity, IAggregateRoot
{
    public string SessionName { get; set; } = string.Empty;    // e.g. "Easy Store Migration - 2024"
    public DateTime AsOfDate { get; set; }                      // Effective date of opening balances
    public OpeningBalanceStatus Status { get; set; } = OpeningBalanceStatus.Pending;

    public int CustomerCount { get; set; }
    public int SupplierCount { get; set; }
    public int InventoryItemCount { get; set; }

    public decimal TotalARBalance { get; set; }    // Total Accounts Receivable imported
    public decimal TotalAPBalance { get; set; }    // Total Accounts Payable imported
    public decimal TotalInventoryValue { get; set; }

    public string? ErrorMessage { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? JournalEntryId { get; set; }    // The generated opening journal entry
}
