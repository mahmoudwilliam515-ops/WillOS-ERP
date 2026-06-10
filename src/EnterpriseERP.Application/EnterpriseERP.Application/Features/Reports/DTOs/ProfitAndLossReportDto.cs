namespace EnterpriseERP.Application.Features.Reports.DTOs;

public class ProfitAndLossReportDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    // Revenue
    public List<AccountLineDto> RevenueLines { get; set; } = new();
    public decimal TotalRevenue { get; set; }

    // Cost of Goods Sold
    public List<AccountLineDto> CogsLines { get; set; } = new();
    public decimal TotalCogs { get; set; }

    public decimal GrossProfit { get; set; }

    // Operating Expenses
    public List<AccountLineDto> ExpenseLines { get; set; } = new();
    public decimal TotalExpenses { get; set; }

    public decimal NetProfit { get; set; }
}

public class AccountLineDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Net { get; set; }
}
