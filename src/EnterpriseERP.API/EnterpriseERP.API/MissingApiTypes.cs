using MediatR;

namespace EnterpriseERP.API;

public record BankStatementImportResultDto
{
    public int TotalTransactions { get; init; }
    public int MatchedTransactions { get; init; }
    public int UnmatchedTransactions { get; init; }
    public decimal TotalAmount { get; init; }
    public List<string> Errors { get; init; } = new();
}

public record CashPositionDto
{
    public Guid CompanyId { get; init; }
    public DateTime AsOfDate { get; init; }
    public decimal TotalCashAndBank { get; init; }
    public List<BankAccountBalanceDto> BankAccounts { get; init; } = new();
    public List<CashAccountBalanceDto> CashAccounts { get; init; } = new();
}

public record BankAccountBalanceDto
{
    public Guid BankAccountId { get; init; }
    public string BankName { get; init; } = default!;
    public string AccountNumber { get; init; } = default!;
    public decimal Balance { get; init; }
    public string Currency { get; init; } = "EGP";
}

public record CashAccountBalanceDto
{
    public Guid CashAccountId { get; init; }
    public string Name { get; init; } = default!;
    public decimal Balance { get; init; }
}

public record GRNDetailDto
{
    public Guid Id { get; init; }
    public string GRNNumber { get; init; } = default!;
    public Guid PurchaseOrderId { get; init; }
    public string Status { get; init; } = default!;
    public DateTime ReceiptDate { get; init; }
    public Guid WarehouseId { get; init; }
    public decimal TotalCost { get; init; }
    public List<GRNLineDetailDto> Lines { get; init; } = new();
}

public record GRNLineDetailDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string ItemCode { get; init; } = default!;
    public string ItemName { get; init; } = default!;
    public decimal OrderedQuantity { get; init; }
    public decimal ReceivedQuantity { get; init; }
    public decimal UnitCost { get; init; }
    public decimal TotalCost { get; init; }
}

public record SalesOrderDetailDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = default!;
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = default!;
    public string Status { get; init; } = default!;
    public DateTime OrderDate { get; init; }
    public DateTime? DeliveryDate { get; init; }
    public decimal TotalAmount { get; init; }
    public bool CreditCheckPassed { get; init; }
    public List<SalesOrderLineDetailDto> Lines { get; init; } = new();
}

public record SalesOrderLineDetailDto
{
    public Guid Id { get; init; }
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = default!;
    public decimal Quantity { get; init; }
    public decimal ShippedQuantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal LineTotal { get; init; }
}

public record BalanceSheetDto
{
    public Guid CompanyId { get; init; }
    public string PeriodName { get; init; } = default!;
    public DateTime AsOfDate { get; init; }
    public List<BalanceSheetSectionDto> Assets { get; init; } = new();
    public List<BalanceSheetSectionDto> Liabilities { get; init; } = new();
    public List<BalanceSheetSectionDto> Equity { get; init; } = new();
    public decimal TotalAssets { get; init; }
    public decimal TotalLiabilitiesAndEquity { get; init; }
    public bool IsBalanced => TotalAssets == TotalLiabilitiesAndEquity;
}

public record BalanceSheetSectionDto
{
    public string SectionName { get; init; } = default!;
    public List<BalanceSheetLineDto> Lines { get; init; } = new();
    public decimal SubTotal { get; init; }
}

public record BalanceSheetLineDto
{
    public string AccountCode { get; init; } = default!;
    public string AccountName { get; init; } = default!;
    public decimal Amount { get; init; }
}

public record CashFlowStatementDto
{
    public Guid CompanyId { get; init; }
    public string PeriodName { get; init; } = default!;
    public List<CashFlowSectionDto> OperatingActivities { get; init; } = new();
    public List<CashFlowSectionDto> InvestingActivities { get; init; } = new();
    public List<CashFlowSectionDto> FinancingActivities { get; init; } = new();
    public decimal NetCashFromOperating { get; init; }
    public decimal NetCashFromInvesting { get; init; }
    public decimal NetCashFromFinancing { get; init; }
    public decimal NetChangeInCash { get; init; }
    public decimal OpeningCashBalance { get; init; }
    public decimal ClosingCashBalance { get; init; }
}

public record CashFlowSectionDto
{
    public string Description { get; init; } = default!;
    public decimal Amount { get; init; }
}

public record ReconcileSubledgerToGLCommand : IRequest<ReconciliationResultDto>
{
    public Guid CompanyId { get; init; }
    public Guid PeriodId { get; init; }
    public string RequestedByUserId { get; init; } = default!;
}

public record ReconciliationResultDto
{
    public bool IsReconciled { get; init; }
    public decimal ARSubledgerTotal { get; init; }
    public decimal APSubledgerTotal { get; init; }
    public decimal ARGLControlTotal { get; init; }
    public decimal APGLControlTotal { get; init; }
    public decimal ARVariance { get; init; }
    public decimal APVariance { get; init; }
    public List<string> Discrepancies { get; init; } = new();
}

public record CreatePaymentProposalCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid TenantId { get; init; }
    public DateTime ProposalDate { get; init; }
    public DateTime PaymentDueBy { get; init; }
    public string RequestedByUserId { get; init; } = default!;
}

public record CreateCreditNoteCommand : IRequest<Guid>
{
    public Guid CompanyId { get; init; }
    public Guid TenantId { get; init; }
    public Guid SalesReturnId { get; init; }
    public Guid SalesInvoiceId { get; init; }
    public Guid CustomerId { get; init; }
    public DateTime CreditNoteDate { get; init; }
    public string Reason { get; init; } = default!;
    public List<CreditNoteLineRequest> Lines { get; init; } = new();
}

public record CreditNote
Set-Content "G:\Mizan\tests\EnterpriseERP.UnitTests\TestTypeAliases.cs" -Encoding UTF8 -Value @'
global using GoodsReceiptLine = EnterpriseERP.Domain.Entities.Purchasing.GoodsReceiptNoteLine;
global using GoodsReceiptNote = EnterpriseERP.Domain.Procurement.GoodsReceiptNote;
