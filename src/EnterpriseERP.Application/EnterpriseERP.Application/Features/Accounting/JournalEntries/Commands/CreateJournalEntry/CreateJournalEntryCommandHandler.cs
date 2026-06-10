using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.Commands.CreateJournalEntry;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, JournalEntryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly EnterpriseERP.Application.Common.Interfaces.Services.IWorkflowService _workflowService;
    private readonly EnterpriseERP.Application.Common.Interfaces.Services.ICurrentUserService _currentUserService;

    public CreateJournalEntryCommandHandler(
        IUnitOfWork unitOfWork, 
        EnterpriseERP.Application.Common.Interfaces.Services.IWorkflowService workflowService,
        EnterpriseERP.Application.Common.Interfaces.Services.ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<JournalEntryDto> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // 1. Verify Accounting Period is Open
        var periods = await _unitOfWork.Repository<AccountingPeriod>()
            .FindAsync(p => p.StartDate.Date <= request.EntryDate.Date && p.EndDate.Date >= request.EntryDate.Date);
            
        var targetPeriod = periods.FirstOrDefault();
        if (targetPeriod == null)
            throw new InvalidOperationException($"No accounting period found for the date {request.EntryDate:yyyy-MM-dd}.");
            
        if (targetPeriod.Status == EnterpriseERP.Domain.Entities.Accounting.AccountingPeriodStatus.Closed)
            throw new InvalidOperationException($"The accounting period '{targetPeriod.PeriodName}' is closed. Cannot post entries to a closed period.");

        var totalDebit = request.Lines.Sum(l => l.DebitAmount);
        var totalCredit = request.Lines.Sum(l => l.CreditAmount);

        // 2. Validate Balancing
        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
            throw new InvalidOperationException($"Journal entry is not balanced. Total Debit: {totalDebit}, Total Credit: {totalCredit}");

        var entryNumber = $"JE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

        var exchangeRate = request.ExchangeRate > 0 ? request.ExchangeRate : 1.0m;

        // 3. Resolve Accounts
        var accountCodes = request.Lines.Select(l => l.AccountCode).Distinct().ToList();
        var accounts = await _unitOfWork.Repository<Account>()
            .FindAsync(a => accountCodes.Contains(a.Code));
            
        var accountDict = accounts.ToDictionary(a => a.Code, a => a);

        // Validation: Ensure all accounts exist and are leaf accounts
        foreach (var code in accountCodes)
        {
            if (!accountDict.TryGetValue(code, out var acc))
                throw new InvalidOperationException($"Account with code '{code}' not found.");
            
            if (!acc.IsLeaf)
                throw new InvalidOperationException($"Account '{acc.Name}' ({code}) is a parent account. Journal entries can only be posted to leaf accounts.");
        }

        var lines = request.Lines.Select(l => new JournalEntryLine
        {
            AccountCode = l.AccountCode,
            AccountName = accountDict[l.AccountCode].Name,
            AccountId = accountDict[l.AccountCode].Id,
            DebitAmount = l.DebitAmount,
            CreditAmount = l.CreditAmount,
            BaseDebitAmount = l.DebitAmount * exchangeRate,
            BaseCreditAmount = l.CreditAmount * exchangeRate,
            Description = l.Description,
            CostCenterId = l.CostCenterId,
            PartyId = l.PartyId
        }).ToList();

        var entry = new JournalEntry
        {
            Id = Guid.NewGuid(),
            EntryNumber = entryNumber,
            EntryDate = request.EntryDate,
            Description = request.Description,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            ReferenceNumber = request.ReferenceNumber,
            CurrencyCode = request.CurrencyCode,
            ExchangeRate = exchangeRate,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            Lines = lines
        };

        // Check Workflow
        var workflowId = await _workflowService.SubmitForApprovalAsync(
            EnterpriseERP.Domain.Entities.Workflow.WorkflowDocumentType.JournalEntry,
            entry.Id,
            entry.EntryNumber,
            totalDebit, // Use total amount as value for workflow thresholds
            _currentUserService.UserId ?? "system",
            cancellationToken
        );

        entry.Status = workflowId == Guid.Empty ? JournalEntryStatus.Posted : JournalEntryStatus.Draft;

        await _unitOfWork.Repository<JournalEntry>().AddAsync(entry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new JournalEntryDto
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            Description = entry.Description,
            ReferenceId = entry.ReferenceId,
            ReferenceType = entry.ReferenceType,
            ReferenceNumber = entry.ReferenceNumber,
            CurrencyCode = entry.CurrencyCode,
            ExchangeRate = entry.ExchangeRate,
            TotalDebit = entry.TotalDebit,
            TotalCredit = entry.TotalCredit,
            Status = entry.Status.ToString(),
            Lines = entry.Lines.Select(l => new JournalEntryLineDto
            {
                Id = l.Id,
                AccountCode = l.AccountCode,
                AccountName = l.AccountName,
                DebitAmount = l.DebitAmount,
                CreditAmount = l.CreditAmount,
                BaseDebitAmount = l.BaseDebitAmount,
                BaseCreditAmount = l.BaseCreditAmount,
                Description = l.Description,
                CostCenterId = l.CostCenterId,
                PartyId = l.PartyId
            }).ToList()
        };
    }
}
