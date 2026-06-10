using MediatR;
using System;
using System.Collections.Generic;

namespace EnterpriseERP.Application.Features.Reports.Queries.GetGlobalAnalytics;

public class GetGlobalAnalyticsQuery : IRequest<GlobalAnalyticsDto>
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class GlobalAnalyticsDto
{
    public decimal TotalRevenueBase { get; set; }
    public decimal TotalExpenseBase { get; set; }
    public decimal NetProfitBase { get; set; }
    public decimal CashBalanceBase { get; set; }
    public decimal ProfitMarginPercentage => TotalRevenueBase > 0 ? (NetProfitBase / TotalRevenueBase) * 100 : 0;
    
    public List<MonthlyTrendDto> RevenueTrend { get; set; } = new();
    public List<AccountCategorySummaryDto> ExpenseBreakdown { get; set; } = new();
    public List<TopPerformanceDto> TopCustomers { get; set; } = new();
    public List<TopPerformanceDto> TopItems { get; set; } = new();
}

public class TopPerformanceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class AccountCategorySummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
