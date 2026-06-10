using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Features.Treasury.PaymentProposals.DTOs;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;

namespace EnterpriseERP.Application.Features.Treasury.PaymentProposals.Queries.GetPaymentProposals;

public class GetPaymentProposalsQuery : IRequest<List<PaymentProposalDto>>
{
    public decimal? MaxTotalBudget { get; set; }
    public Guid? SupplierId { get; set; }
}

public class GetPaymentProposalsQueryHandler : IRequestHandler<GetPaymentProposalsQuery, List<PaymentProposalDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPaymentProposalsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PaymentProposalDto>> Handle(GetPaymentProposalsQuery request, CancellationToken cancellationToken)
    {
        // Query unpaid and approved invoices
        var invoices = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(i => 
            (i.Status == PurchaseInvoiceStatus.Approved || i.Status == PurchaseInvoiceStatus.ApprovedForPayment) &&
            i.RemainingAmount > 0 &&
            (!request.SupplierId.HasValue || i.SupplierId == request.SupplierId.Value));

        var proposals = new List<PaymentProposalDto>();
        decimal currentBudget = 0;

        // Simple logic for proposals:
        // 1. Prioritize older invoices (FIFO)
        // 2. Prioritize invoices near or past due date (assuming 30 days credit if not specified)
        
        var sortedInvoices = invoices.OrderBy(i => i.InvoiceDate).ToList();

        foreach (var inv in sortedInvoices)
        {
            var dueDate = inv.InvoiceDate.AddDays(30); // Default 30 days
            var isOverdue = DateTime.UtcNow > dueDate;
            
            var suggestedAmount = inv.RemainingAmount;
            
            // If budget is constrained
            if (request.MaxTotalBudget.HasValue && currentBudget + suggestedAmount > request.MaxTotalBudget.Value)
            {
                suggestedAmount = Math.Max(0, request.MaxTotalBudget.Value - currentBudget);
            }

            if (suggestedAmount <= 0 && request.MaxTotalBudget.HasValue) continue;

            proposals.Add(new PaymentProposalDto
            {
                InvoiceId = inv.Id,
                InvoiceNumber = inv.InvoiceNumber,
                SupplierName = inv.Supplier?.Name ?? "N/A",
                InvoiceDate = inv.InvoiceDate,
                DueDate = dueDate,
                TotalAmount = inv.TotalAmount,
                RemainingAmount = inv.RemainingAmount,
                SuggestedPaymentAmount = suggestedAmount,
                Priority = isOverdue ? 1 : 2,
                RecommendationReason = isOverdue ? "Invoice is overdue" : "Regular payment schedule"
            });

            currentBudget += suggestedAmount;
            
            if (request.MaxTotalBudget.HasValue && currentBudget >= request.MaxTotalBudget.Value)
                break;
        }

        return proposals;
    }
}
