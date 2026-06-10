using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.EInvoice;
using EnterpriseERP.SharedKernel.Common;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.EInvoice.Commands;

public class SubmitEtaInvoiceCommand : IRequest<Guid>
{
    public Guid SalesInvoiceId { get; set; }
}

public class SubmitEtaInvoiceCommandHandler : IRequestHandler<SubmitEtaInvoiceCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public SubmitEtaInvoiceCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SubmitEtaInvoiceCommand request, CancellationToken cancellationToken)
    {
        // Mock submission to Egypt Tax Authority (ETA)
        var invoiceDoc = new EInvoiceDocument(
            request.SalesInvoiceId, 
            "SalesInvoice", 
            EInvoiceProvider.ETA,
            ""); // No PIH in ETA typically

        // Mock ETA response
        string etaSubmissionId = Guid.NewGuid().ToString("N");
        
        invoiceDoc.UpdateStatus(EInvoiceStatus.Cleared, "Valid", $"Submitted to ETA successfully. SubmissionId: {etaSubmissionId}");

        await _unitOfWork.Repository<EInvoiceDocument>().AddAsync(invoiceDoc);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return invoiceDoc.Id;
    }
}
