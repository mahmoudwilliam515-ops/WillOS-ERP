using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Commands.ApproveDebitNote;

public record ApproveDebitNoteCommand(Guid DebitNoteId) : IRequest<Result<bool>>;

public class ApproveDebitNoteCommandHandler : IRequestHandler<ApproveDebitNoteCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveDebitNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ApproveDebitNoteCommand request, CancellationToken cancellationToken)
    {
        var debitNote = await _unitOfWork.Repository<DebitNote>().GetByIdAsync(request.DebitNoteId);

        if (debitNote == null)
            return Result.Failure<bool>(new Error("DebitNote.NotFound", "Debit Note not found."));

        if (debitNote.Status == EnterpriseERP.Domain.Procurement.DebitNoteStatus.Approved) // Already Approved
            return Result.Success(true);

        debitNote.Status = EnterpriseERP.Domain.Procurement.DebitNoteStatus.Approved; // Approved
        _unitOfWork.Repository<DebitNote>().Update(debitNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
