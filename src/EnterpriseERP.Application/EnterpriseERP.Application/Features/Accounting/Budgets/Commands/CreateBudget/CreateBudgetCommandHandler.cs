using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Accounting.Budgets.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Accounting.Budgets.Commands.CreateBudget;

public class CreateBudgetCommandHandler : IRequestHandler<CreateBudgetCommand, Result<BudgetDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBudgetCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BudgetDto>> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        // Validate fiscal year exists
        var fiscalYear = await _unitOfWork.Repository<FiscalYear>().GetByIdAsync(request.FiscalYearId);
        if (fiscalYear == null)
            return Result.Failure<BudgetDto>(new Error("Budget.FiscalYearNotFound", "Fiscal year not found."));

        if (fiscalYear.Status == FiscalYearStatus.Closed)
            return Result.Failure<BudgetDto>(new Error("Budget.FiscalYearClosed", "Cannot create a budget for a closed fiscal year."));

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            FiscalYearId = request.FiscalYearId,
            CostCenterId = request.CostCenterId,
            Status = BudgetStatus.Draft,
            Notes = request.Notes,
            Lines = request.Lines.Select(l => new BudgetLine
            {
                Id = Guid.NewGuid(),
                AccountId = l.AccountId,
                Jan = l.Jan, Feb = l.Feb, Mar = l.Mar,
                Apr = l.Apr, May = l.May, Jun = l.Jun,
                Jul = l.Jul, Aug = l.Aug, Sep = l.Sep,
                Oct = l.Oct, Nov = l.Nov, Dec = l.Dec,
                Notes = l.Notes
            }).ToList()
        };

        await _unitOfWork.Repository<Budget>().AddAsync(budget);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new BudgetDto
        {
            Id = budget.Id,
            Name = budget.Name,
            FiscalYearId = budget.FiscalYearId,
            FiscalYearName = fiscalYear.Name,
            CostCenterId = budget.CostCenterId,
            Status = budget.Status.ToString(),
            Notes = budget.Notes,
            Lines = budget.Lines.Select(l => new BudgetLineDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                Jan = l.Jan, Feb = l.Feb, Mar = l.Mar,
                Apr = l.Apr, May = l.May, Jun = l.Jun,
                Jul = l.Jul, Aug = l.Aug, Sep = l.Sep,
                Oct = l.Oct, Nov = l.Nov, Dec = l.Dec,
                TotalPlanned = l.TotalPlanned,
                Notes = l.Notes
            }).ToList()
        });
    }
}
