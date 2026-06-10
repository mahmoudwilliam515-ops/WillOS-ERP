namespace EnterpriseERP.Application.Features.Accounting.Budgets.DTOs;

public class BudgetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid FiscalYearId { get; set; }
    public string FiscalYearName { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<BudgetLineDto> Lines { get; set; } = new();
}

public class BudgetLineDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
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
    public decimal TotalPlanned { get; set; }
    public string? Notes { get; set; }
}

public class BudgetVsActualDto
{
    public Guid BudgetId { get; set; }
    public string BudgetName { get; set; } = string.Empty;
    public string FiscalYearName { get; set; } = string.Empty;
    public List<BudgetVsActualLineDto> Lines { get; set; } = new();
    public decimal TotalPlanned { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalVariance { get; set; }
    public decimal VariancePercent { get; set; }
}

public class BudgetVsActualLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal PlannedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercent { get; set; }
}
