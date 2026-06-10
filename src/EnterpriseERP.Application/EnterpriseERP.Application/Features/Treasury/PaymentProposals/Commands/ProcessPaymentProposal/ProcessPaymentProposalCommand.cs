using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Exceptions;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentProposals.Commands.ProcessPaymentProposal;

public class ProcessPaymentProposalCommand : IRequest<List<Guid>>
{
    public List<ProposalSelection> Selections { get; set; } = new();
    public Guid? CashAccountId { get; set; }
    public Guid? BankAccountId { get; set; }
    public DateTime VoucherDate { get; set; } = DateTime.UtcNow;
}

public class ProposalSelection
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
}

public class ProcessPaymentProposalCommandHandler : IRequestHandler<ProcessPaymentProposalCommand, List<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProcessPaymentProposalCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Guid>> Handle(ProcessPaymentProposalCommand request, CancellationToken cancellationToken)
    {
        if (request.CashAccountId == null && request.BankAccountId == null)
            throw new AccountingDomainException("Must specify either a Cash Account or a Bank Account.");

        var createdVoucherIds = new List<Guid>();

        // Group selections by supplier to create one voucher per supplier if possible
        // (Or one voucher per invoice, depending on business rule. Usually one per supplier is better for bank transfers)
        
        var invoices = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(i => 
            request.Selections.Select(s => s.InvoiceId).Contains(i.Id));

        var groupedBySupplier = invoices.GroupBy(i => i.SupplierId);

        foreach (var group in groupedBySupplier)
        {
            var supplierId = group.Key;
            var supplierInvoices = group.ToList();
            var supplierSelections = request.Selections.Where(s => supplierInvoices.Any(i => i.Id == s.InvoiceId)).ToList();
            var totalAmount = supplierSelections.Sum(s => s.Amount);

            var voucherId = Guid.NewGuid();
            var voucher = new PaymentVoucher
            {
                Id = voucherId,
                VoucherNumber = $"PV-PROP-{DateTime.UtcNow:yyyyMMddHHmmss}-{supplierId.ToString().Substring(0,4)}",
                VoucherDate = request.VoucherDate,
                SupplierId = supplierId,
                CashAccountId = request.CashAccountId,
                BankAccountId = request.BankAccountId,
                Amount = totalAmount,
                Status = VoucherStatus.Draft,
                InvoicePayments = supplierSelections.Select(s => new InvoicePayment
                {
                    PaymentVoucherId = voucherId,
                    PurchaseInvoiceId = s.InvoiceId,
                    Amount = s.Amount,
                    SettlementDate = request.VoucherDate
                }).ToList()
            };

            await _unitOfWork.Repository<PaymentVoucher>().AddAsync(voucher);
            createdVoucherIds.Add(voucherId);
        }

        await _unitOfWork.SaveChangesAsync();
        return createdVoucherIds;
    }
}
