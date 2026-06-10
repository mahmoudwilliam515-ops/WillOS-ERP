using EnterpriseERP.Application.Features.Accounting.JournalEntries.DTOs;
using FluentValidation;

namespace EnterpriseERP.Application.Features.Accounting.JournalEntries.Commands.CreateJournalEntry;

public class CreateJournalEntryCommandValidator : AbstractValidator<CreateJournalEntryCommand>
{
    public CreateJournalEntryCommandValidator()
    {
        RuleFor(v => v.EntryDate).NotEmpty().WithMessage("Entry date is required.");
        RuleFor(v => v.Description).NotEmpty().WithMessage("Description is required.");
        
        RuleFor(v => v.Lines)
            .NotEmpty().WithMessage("Journal entry must have at least two lines.")
            .Must(lines => lines != null && lines.Count >= 2).WithMessage("Double-entry accounting requires at least two lines.");

        RuleForEach(v => v.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountCode).NotEmpty().WithMessage("Account code is required.");
            line.RuleFor(l => l.AccountName).NotEmpty().WithMessage("Account name is required.");
            
            line.RuleFor(l => l)
                .Must(l => (l.DebitAmount > 0 && l.CreditAmount == 0) || (l.DebitAmount == 0 && l.CreditAmount > 0))
                .WithMessage("A line must be either debit or credit, not both or neither.");
        });

        // Enforce Double-Entry Rule (Total Debit == Total Credit)
        RuleFor(v => v)
            .Must(v => 
            {
                if (v.Lines == null) return false;
                var totalDebit = v.Lines.Sum(l => l.DebitAmount);
                var totalCredit = v.Lines.Sum(l => l.CreditAmount);
                return totalDebit == totalCredit && totalDebit > 0;
            })
            .WithMessage("Total Debit must equal Total Credit, and must be greater than zero.");
    }
}
