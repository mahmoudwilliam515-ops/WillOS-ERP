using System;

namespace EnterpriseERP.Application.Features.Treasury.PaymentProposals.DTOs;

public class PaymentProposalDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal SuggestedPaymentAmount { get; set; }
    public int Priority { get; set; } // 1: High, 2: Medium, 3: Low
    public string RecommendationReason { get; set; } = string.Empty;
}
