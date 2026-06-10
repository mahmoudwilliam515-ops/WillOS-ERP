using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesReturns.Commands.ApproveCreditNote;

public record ApproveCreditNoteCommand(Guid CreditNoteId) : IRequest<Result<bool>>;

public class ApproveCreditNoteCommandHandler : IRequestHandler<ApproveCreditNoteCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveCreditNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(ApproveCreditNoteCommand request, CancellationToken cancellationToken)
    {
        var creditNote = await _unitOfWork.Repository<CreditNote>().GetByIdAsync(request.CreditNoteId);

        if (creditNote == null)
            return Result.Failure<bool>(new Error("CreditNote.NotFound", "Credit Note not found."));

        if (creditNote.Status == 1) // Already Approved
            return Result.Success(true);

        creditNote.Status = 1; // Approved
        _unitOfWork.Repository<CreditNote>().Update(creditNote);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
