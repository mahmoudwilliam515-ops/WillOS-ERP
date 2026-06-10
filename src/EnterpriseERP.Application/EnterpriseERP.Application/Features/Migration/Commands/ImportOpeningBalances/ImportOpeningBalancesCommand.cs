using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Migration;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Migration.Commands.ImportOpeningBalances;

// ============================================================
// DTOs for the import payload
// ============================================================

public record CustomerOpeningBalanceDto(
    Guid CustomerId,
    string CustomerName,
    decimal Balance    // Positive = owes us (Debit on AR)
);

public record SupplierOpeningBalanceDto(
    Guid SupplierId,
    string SupplierName,
    decimal Balance    // Positive = we owe them (Credit on AP)
);

public record InventoryOpeningBalanceDto(
    Guid ItemId,
    Guid WarehouseId,
    decimal Quantity,
    decimal UnitCost
);

// ============================================================
// Command
// ============================================================

public record ImportOpeningBalancesCommand(
    string SessionName,
    DateTime AsOfDate,
    List<CustomerOpeningBalanceDto> Customers,
    List<SupplierOpeningBalanceDto> Suppliers,
    List<InventoryOpeningBalanceDto> InventoryItems,
    bool BypassStockGuard = true,   // Always true for historical migrations
    string Notes = ""
) : IRequest<ImportOpeningBalancesResult>;

public class ImportOpeningBalancesResult
{
    public Guid SessionId { get; set; }
    public string JournalEntryNumber { get; set; } = string.Empty;
    public int CustomersImported { get; set; }
    public int SuppliersImported { get; set; }
    public int InventoryItemsImported { get; set; }
    public decimal TotalARBalance { get; set; }
    public decimal TotalAPBalance { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public bool Success { get; set; }
}

// ============================================================
// Handler — the core migration engine
// ============================================================

public class ImportOpeningBalancesCommandHandler
    : IRequestHandler<ImportOpeningBalancesCommand, ImportOpeningBalancesResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public ImportOpeningBalancesCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportOpeningBalancesResult> Handle(
        ImportOpeningBalancesCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Guard: no duplicate sessions with same name
        var existing = await _unitOfWork.Repository<OpeningBalanceSession>()
            .FindAsync(s => s.SessionName == request.SessionName);

        if (existing.Any(s => s.Status == OpeningBalanceStatus.Completed))
            throw new AccountingDomainException(
                $"Opening balance session '{request.SessionName}' has already been completed. " +
                "To re-import, reverse the previous session first.");

        var session = new OpeningBalanceSession
        {
            Id = Guid.NewGuid(),
            SessionName = request.SessionName,
            AsOfDate = request.AsOfDate.ToUniversalTime(),
            Status = OpeningBalanceStatus.Processing,
            Notes = request.Notes,
            CustomerCount = request.Customers.Count,
            SupplierCount = request.Suppliers.Count,
            InventoryItemCount = request.InventoryItems.Count,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<OpeningBalanceSession>().AddAsync(session);

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // 2. Build the Opening Journal Entry
            var journalEntry = BuildOpeningJournalEntry(request, session.Id);
            await _unitOfWork.Repository<JournalEntry>().AddAsync(journalEntry);
            foreach (var line in journalEntry.Lines)
                await _unitOfWork.Repository<JournalEntryLine>().AddAsync(line);

            // 3. Post Inventory Opening Balances as AdjustmentIn transactions
            var totalInventoryValue = 0m;
            foreach (var item in request.InventoryItems)
            {
                var totalCost = item.Quantity * item.UnitCost;
                totalInventoryValue += totalCost;

                var invTx = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    ItemId = item.ItemId,
                    WarehouseId = item.WarehouseId,
                    Type = TransactionType.AdjustmentIn,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    TotalCost = totalCost,
                    TransactionDate = request.AsOfDate.ToUniversalTime(),
                    ReferenceType = "OpeningBalance",
                    ReferenceNumber = session.SessionName,
                    Notes = $"Opening balance import — session: {session.SessionName}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(invTx);
            }

            // 4. Finalize session
            session.Status = OpeningBalanceStatus.Completed;
            session.JournalEntryId = journalEntry.Id;
            session.TotalARBalance = request.Customers.Sum(c => c.Balance);
            session.TotalAPBalance = request.Suppliers.Sum(s => s.Balance);
            session.TotalInventoryValue = totalInventoryValue;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<OpeningBalanceSession>().Update(session);

            await _unitOfWork.CommitTransactionAsync();

            return new ImportOpeningBalancesResult
            {
                SessionId = session.Id,
                JournalEntryNumber = journalEntry.EntryNumber,
                CustomersImported = request.Customers.Count,
                SuppliersImported = request.Suppliers.Count,
                InventoryItemsImported = request.InventoryItems.Count,
                TotalARBalance = session.TotalARBalance,
                TotalAPBalance = session.TotalAPBalance,
                TotalInventoryValue = totalInventoryValue,
                Success = true
            };
        }
        catch
        {
            session.Status = OpeningBalanceStatus.Failed;
            session.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<OpeningBalanceSession>().Update(session);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private JournalEntry BuildOpeningJournalEntry(
        ImportOpeningBalancesCommand request, Guid sessionId)
    {
        var totalAR = request.Customers.Sum(c => c.Balance);
        var totalAP = request.Suppliers.Sum(s => s.Balance);
        var totalInventory = request.InventoryItems.Sum(i => i.Quantity * i.UnitCost);

        // Opening equity balancer: Retained Earnings absorbs the difference
        var totalDebits = totalAR + totalInventory;
        var totalCredits = totalAP;
        var equityBalance = totalDebits - totalCredits; // goes to Retained Earnings (Credit)

        var lines = new List<JournalEntryLine>();

        // Debit: Accounts Receivable per customer (sub-ledger)
        foreach (var c in request.Customers.Where(c => c.Balance != 0))
        {
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                AccountCode = "1200",
                AccountName = "Accounts Receivable",
                DebitAmount = c.Balance > 0 ? c.Balance : 0,
                CreditAmount = c.Balance < 0 ? Math.Abs(c.Balance) : 0,
                PartyId = c.CustomerId,
                Description = $"Opening balance — {c.CustomerName}"
            });
        }

        // Credit: Accounts Payable per supplier (sub-ledger)
        foreach (var s in request.Suppliers.Where(s => s.Balance != 0))
        {
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                AccountCode = "2100",
                AccountName = "Accounts Payable",
                DebitAmount = s.Balance < 0 ? Math.Abs(s.Balance) : 0,
                CreditAmount = s.Balance > 0 ? s.Balance : 0,
                PartyId = s.SupplierId,
                Description = $"Opening balance — {s.SupplierName}"
            });
        }

        // Debit: Inventory (bulk total)
        if (totalInventory > 0)
        {
            lines.Add(new JournalEntryLine
            {
                Id = Guid.NewGuid(),
                AccountCode = "1300",
                AccountName = "Inventory",
                DebitAmount = totalInventory,
                CreditAmount = 0,
                Description = $"Opening stock value — {request.InventoryItems.Count} items"
            });
        }

        // Credit: Retained Earnings (balancer — ensures double-entry integrity)
        lines.Add(new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            AccountCode = "3200",
            AccountName = "Retained Earnings / Opening Equity",
            DebitAmount = equityBalance < 0 ? Math.Abs(equityBalance) : 0,
            CreditAmount = equityBalance > 0 ? equityBalance : 0,
            Description = "Opening balance equity offset"
        });

        // Validate double entry
        var debits = lines.Sum(l => l.DebitAmount);
        var credits = lines.Sum(l => l.CreditAmount);
        if (debits != credits)
            throw new AccountingDomainException(
                $"Opening balance journal entry is unbalanced. Debits: {debits}, Credits: {credits}");

        var journalEntry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = $"OB-{request.AsOfDate:yyyyMMdd}-{sessionId.ToString()[..8]}",
            EntryDate = request.AsOfDate.ToUniversalTime(),
            Description = $"Opening Balances — {request.SessionName}",
            ReferenceId = sessionId,
            ReferenceType = "OpeningBalanceSession",
            ReferenceNumber = request.SessionName,
            TotalDebit = debits,
            TotalCredit = credits,
            Status = JournalEntryStatus.Posted,
            Lines = lines,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Propagate FK so EF InMemory can query lines independently
        foreach (var line in journalEntry.Lines)
            line.JournalEntryId = journalEntry.Id;

        return journalEntry;
    }
}
