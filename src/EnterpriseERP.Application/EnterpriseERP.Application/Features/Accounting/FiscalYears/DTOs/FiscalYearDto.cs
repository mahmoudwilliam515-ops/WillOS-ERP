namespace EnterpriseERP.Application.Features.Accounting.FiscalYears.DTOs;

public class AccountingPeriodDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
}

public class FiscalYearDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
    public List<AccountingPeriodDto> Periods { get; set; } = new();
}
