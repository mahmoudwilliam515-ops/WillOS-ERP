using EnterpriseERP.Application.Features.Accounting.Budgets.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.Budgets.Queries.GetBudgetVsActual;

public class GetBudgetVsActualQuery : IRequest<Result<BudgetVsActualDto>>
{
    public Guid BudgetId { get; set; }
}
