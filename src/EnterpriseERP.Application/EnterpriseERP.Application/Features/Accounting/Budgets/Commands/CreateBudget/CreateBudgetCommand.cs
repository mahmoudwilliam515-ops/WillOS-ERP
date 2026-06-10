using EnterpriseERP.Application.Features.Accounting.Budgets.DTOs;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.Budgets.Commands.CreateBudget;

public class CreateBudgetCommand : IRequest<Result<BudgetDto>>
{
    public string Name { get; set; } = string.Empty;
    public Guid FiscalYearId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? Notes { get; set; }
    public List<CreateBudgetLineDto> Lines { get; set; } = new();
}

public class CreateBudgetLineDto
{
    public Guid AccountId { get; set; }
    public decimal Jan { get; set; }
    public decimal Feb { get; set; }
    public decimal Mar { get; set; }
    public decimal Apr { get; set; }
    public decimal May { get; set; }
    public decimal Jun { get; set; }
    public decimal Jul { get; set; }
    public decimal Aug { get; set; }
    public decimal Sep { get; set; }
    public decimal Oct { get; set; }
    public decimal Nov { get; set; }
    public decimal Dec { get; set; }
    public string? Notes { get; set; }
}
