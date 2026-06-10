using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.Commands.CloseAccountingPeriod;

public class CloseAccountingPeriodCommand : IRequest<bool>
{
    public Guid PeriodId { get; set; }
}
