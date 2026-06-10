using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.SalesQuotations.Commands.CreateSalesQuotation;

public class CreateSalesQuotationCommandHandler : IRequestHandler<CreateSalesQuotationCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalesQuotationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateSalesQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = new SalesQuotation
        {
            QuotationNumber = $"SQ-{DateTime.UtcNow:yyyyMMddHHmmss}",
            QuotationDate = request.QuotationDate,
            ValidUntil = request.ValidUntil,
            CustomerId = request.CustomerId,
            BranchId = request.BranchId,
            SubTotal = request.SubTotal,
            DiscountAmount = request.DiscountAmount,
            TaxAmount = request.TaxAmount,
            TotalAmount = request.TotalAmount,
            Notes = request.Notes,
            Status = QuotationStatus.Draft
        };

        await _unitOfWork.Repository<SalesQuotation>().AddAsync(quotation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(quotation.Id);
    }
}
