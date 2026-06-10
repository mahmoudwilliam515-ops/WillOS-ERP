using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Treasury;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentProposals.Commands.GenerateBankPaymentFile;

public class GenerateBankPaymentFileCommand : IRequest<string>
{
    public List<Guid> VoucherIds { get; set; } = new();
}

public class GenerateBankPaymentFileCommandHandler : IRequestHandler<GenerateBankPaymentFileCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIso20022Service _iso20022Service;

    public GenerateBankPaymentFileCommandHandler(IUnitOfWork unitOfWork, IIso20022Service iso20022Service)
    {
        _unitOfWork = unitOfWork;
        _iso20022Service = iso20022Service;
    }

    public async Task<string> Handle(GenerateBankPaymentFileCommand request, CancellationToken cancellationToken)
    {
        var vouchers = await _unitOfWork.Repository<PaymentVoucher>().FindAsync(v =>
            request.VoucherIds.Contains(v.Id));

        foreach (var v in vouchers)
        {
            if (v.BankAccountId.HasValue && v.BankAccount == null)
            {
                v.BankAccount = await _unitOfWork.Repository<BankAccount>().GetByIdAsync(v.BankAccountId.Value);
            }
        }

        return await _iso20022Service.GeneratePain001XmlAsync(vouchers);
    }
}
