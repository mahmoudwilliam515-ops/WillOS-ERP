using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;

namespace EnterpriseERP.Application.Features.Accounting.Commands.CloseAccountingPeriod;

public class CloseAccountingPeriodCommand : IRequest<Result<Guid>>
{
    public Guid PeriodId { get; set; }
}
