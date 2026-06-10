using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.PurchaseReturns.Commands.CreateDebitNote;

public class CreateDebitNoteCommand : IRequest<Result<Guid>>
{
    public DateTime NoteDate { get; set; } = DateTime.UtcNow;
    public Guid SupplierId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateDebitNoteCommandHandler : IRequestHandler<CreateDebitNoteCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateDebitNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateDebitNoteCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            var debitNote = new DebitNote
            {
                Id = Guid.NewGuid(),
                NoteNumber = $"DN-M-{DateTime.UtcNow:yyyyMMddHHmmss}", // M for Manual
                NoteDate = request.NoteDate,
                SupplierId = request.SupplierId,
                Amount = request.Amount,
                Reason = request.Reason,
                Status = 0 // Draft
            };

            await _unitOfWork.Repository<DebitNote>().AddAsync(debitNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(debitNote.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("DebitNote.CreateError", ex.Message));
        }
    }
}
