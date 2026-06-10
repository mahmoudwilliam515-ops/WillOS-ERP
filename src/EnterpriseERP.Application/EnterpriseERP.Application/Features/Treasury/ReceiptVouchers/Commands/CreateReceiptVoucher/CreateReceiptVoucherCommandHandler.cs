using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Treasury;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.ReceiptVouchers.Commands.CreateReceiptVoucher;

public class CreateReceiptVoucherCommandHandler : IRequestHandler<CreateReceiptVoucherCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateReceiptVoucherCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        if (request.CashAccountId == null && request.BankAccountId == null)
            throw new Exception("Must specify either a Cash Account or a Bank Account.");

        var tenantId = _currentUserService.TenantId ?? Guid.Empty;

        var voucher = new ReceiptVoucher
        {
            Id = Guid.NewGuid(),
            VoucherNumber = $"RV-{DateTime.UtcNow:yyyyMMddHHmmss}",
            VoucherDate = request.VoucherDate,
            CustomerId = request.CustomerId,
            CashAccountId = request.CashAccountId,
            BankAccountId = request.BankAccountId,
            Amount = request.Amount,
            ReferenceNumber = request.ReferenceNumber,
            Notes = request.Notes,
            Status = VoucherStatus.Draft,
            TenantId = tenantId,
            InvoicePayments = request.Allocations.Select(a => InvoicePayment.Create(
                salesInvoiceId: a.InvoiceId,
                receiptVoucherId: Guid.Empty, // Will be set by EF
                allocatedAmount: a.Amount,
                settlementDate: request.VoucherDate,
                tenantId: tenantId
            )).ToList()
        };

        await _unitOfWork.Repository<ReceiptVoucher>().AddAsync(voucher);
        await _unitOfWork.SaveChangesAsync();

        return voucher.Id;
    }
}
