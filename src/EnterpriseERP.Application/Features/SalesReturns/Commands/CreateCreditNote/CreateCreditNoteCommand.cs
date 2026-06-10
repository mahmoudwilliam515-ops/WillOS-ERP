using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesReturns.Commands.CreateCreditNote;

public class CreateCreditNoteCommand : IRequest<Result<Guid>>
{
    public DateTime NoteDate { get; set; } = DateTime.UtcNow;
    public Guid CustomerId { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateCreditNoteCommandHandler : IRequestHandler<CreateCreditNoteCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCreditNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCreditNoteCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            var creditNote = new CreditNote
            {
                Id = Guid.NewGuid(),
                NoteNumber = $"CN-M-{DateTime.UtcNow:yyyyMMddHHmmss}", // M for Manual
                NoteDate = request.NoteDate,
                CustomerId = request.CustomerId,
                Amount = request.Amount,
                Reason = request.Reason,
                Status = 0 // Draft
            };

            await _unitOfWork.Repository<CreditNote>().AddAsync(creditNote);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(creditNote.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("CreditNote.CreateError", ex.Message));
        }
    }
}
