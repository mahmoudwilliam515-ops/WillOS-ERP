namespace EnterpriseERP.Application.Features.Reports.DTOs;

public class TrialBalanceReportDto
{
    public DateTime AsOfDate { get; set; }
    public List<TrialBalanceLine> Lines { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class TrialBalanceLine
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal Balance { get; set; } // Debit - Credit
}
