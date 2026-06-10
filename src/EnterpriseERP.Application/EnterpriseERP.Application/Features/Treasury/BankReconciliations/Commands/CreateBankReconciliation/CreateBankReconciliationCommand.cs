using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Treasury;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.BankReconciliations.Commands.CreateBankReconciliation;

public class CreateBankReconciliationCommand : IRequest<Guid>
{
    public Guid BankAccountId { get; set; }
    public DateTime StatementDate { get; set; }
    public decimal StatementEndingBalance { get; set; }
    public List<BankReconciliationLineDto> Lines { get; set; } = new();
}

public class BankReconciliationLineDto
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public bool IsCleared { get; set; }
}

public class CreateBankReconciliationCommandHandler : IRequestHandler<CreateBankReconciliationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBankReconciliationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBankReconciliationCommand request, CancellationToken cancellationToken)
    {
        var lines = request.Lines.Select(l => new BankReconciliationLine
        {
            Id = Guid.NewGuid(),
            TransactionId = l.TransactionId,
            Amount = l.Amount,
            IsCleared = l.IsCleared
        }).ToList();

        var clearedBalance = lines.Where(l => l.IsCleared).Sum(l => l.Amount);

        var reconciliation = new BankReconciliation
        {
            Id = Guid.NewGuid(),
            BankAccountId = request.BankAccountId,
            StatementDate = request.StatementDate,
            StatementEndingBalance = request.StatementEndingBalance,
            ClearedBalance = clearedBalance,
            IsClosed = false,
            Lines = lines
        };

        await _unitOfWork.Repository<BankReconciliation>().AddAsync(reconciliation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return reconciliation.Id;
    }
}
