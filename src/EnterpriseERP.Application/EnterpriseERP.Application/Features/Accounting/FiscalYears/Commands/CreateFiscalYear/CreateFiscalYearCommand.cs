using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.Commands.CreateFiscalYear;

public class CreateFiscalYearCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
