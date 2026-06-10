using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Commands.ReverseSalesInvoice;

public record ReverseSalesInvoiceCommand(Guid InvoiceId, string Reason) : IRequest<bool>;

public class ReverseSalesInvoiceCommandHandler : IRequestHandler<ReverseSalesInvoiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ReverseSalesInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ReverseSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var invoice = await _unitOfWork.Repository<SalesInvoice>().GetByIdAsync(request.InvoiceId);

            if (invoice == null)
                throw new SalesDomainException($"Sales invoice with id {request.InvoiceId} not found");

            if (invoice.Status != InvoiceStatus.Approved)
                throw new SalesDomainException("Only approved invoices can be reversed.");

            // 1. Change Status
            invoice.Status = InvoiceStatus.Cancelled;
            invoice.Notes = $"{invoice.Notes} | Reversed on {DateTime.UtcNow:yyyy-MM-dd}: {request.Reason}";
            _unitOfWork.Repository<SalesInvoice>().Update(invoice);

            // 2. Reverse Inventory Transactions
            var originalInvTxs = await _unitOfWork.Repository<InventoryTransaction>()
                .FindAsync(t => t.ReferenceId == invoice.Id && t.ReferenceType == "SalesInvoice");

            foreach (var origTx in originalInvTxs)
            {
                var reversalTx = new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    ItemId = origTx.ItemId,
                    WarehouseId = origTx.WarehouseId,
                    ReferenceId = invoice.Id,
                    ReferenceType = "SalesInvoiceReversal",
                    ReferenceNumber = $"REV-{origTx.ReferenceNumber}",
                    Type = TransactionType.ReturnFromCustomer, // Inverse of SalesOut
                    Quantity = origTx.Quantity * -1, // Original was negative, so this becomes positive
                    UnitCost = origTx.UnitCost,
                    TotalCost = origTx.TotalCost * -1,
                    TransactionDate = DateTime.UtcNow,
                    Notes = $"Reversal of tx {origTx.Id}: {request.Reason}"
                };
                await _unitOfWork.Repository<InventoryTransaction>().AddAsync(reversalTx);
            }

            // 3. Reverse Journal Entry
            var originalJe = (await _unitOfWork.Repository<JournalEntry>()
                .FindAsync(j => j.ReferenceId == invoice.Id && j.ReferenceType == "SalesInvoice")).FirstOrDefault();

            if (originalJe != null && !originalJe.IsReversed)
            {
                // Fetch lines
                var lines = await _unitOfWork.Repository<JournalEntryLine>()
                    .FindAsync(l => l.JournalEntryId == originalJe.Id);

                var reversalJe = new JournalEntry
                {
                    Id = Guid.NewGuid(),
                    EntryNumber = $"REV-{originalJe.EntryNumber}",
                    EntryDate = DateTime.UtcNow,
                    Description = $"Reversal of {originalJe.EntryNumber}: {request.Reason}",
                    ReferenceId = invoice.Id,
                    ReferenceType = "SalesInvoiceReversal",
                    ReferenceNumber = invoice.InvoiceNumber,
                    TotalDebit = originalJe.TotalCredit, // Swapped
                    TotalCredit = originalJe.TotalDebit, // Swapped
                    Status = JournalEntryStatus.Posted,
                    Lines = lines.Select(l => new JournalEntryLine
                    {
                        Id = Guid.NewGuid(),
                        AccountCode = l.AccountCode,
                        AccountName = l.AccountName,
                        DebitAmount = l.CreditAmount, // Swap Debit/Credit
                        CreditAmount = l.DebitAmount,
                        PartyId = l.PartyId,
                        Description = $"Reversal: {l.Description}"
                    }).ToList()
                };

                await _unitOfWork.Repository<JournalEntry>().AddAsync(reversalJe);

                originalJe.IsReversed = true;
                originalJe.ReversedByEntryId = reversalJe.Id;
                _unitOfWork.Repository<JournalEntry>().Update(originalJe);
            }

            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
