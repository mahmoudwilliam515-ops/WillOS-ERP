using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Treasury;

/// <summary>
/// مقترح الدفع الدفعي — يجمع الفواتير المستحقة للموافقة عليها دفعة واحدة
/// Blueprint Section 1.4 — Payment Proposal Batch
/// القاعدة: يجب موافقة CFO قبل إرسال XML للبنك
/// </summary>
public class PaymentProposal : AuditableEntity
{
    public Guid Id { get; private set; }
    public string ProposalNumber { get; private set; } = default!;
    public Guid CompanyId { get; private set; }
    public Guid BankAccountId { get; private set; }
    public DateTime ProposalDate { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public PaymentProposalStatus Status { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }

    private readonly List<PaymentProposalLine> _lines = new();
    public IReadOnlyCollection<PaymentProposalLine> Lines => _lines.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(l => l.Amount);

    private PaymentProposal() { }

    public static PaymentProposal Create(
        Guid companyId,
        Guid bankAccountId,
        DateTime proposalDate,
        DateTime paymentDate,
        string proposalNumber)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId is required.", nameof(companyId));
        if (paymentDate < proposalDate)
            throw new ArgumentException("PaymentDate cannot be before ProposalDate.");

        return new PaymentProposal
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            BankAccountId = bankAccountId,
            ProposalDate = proposalDate,
            PaymentDate = paymentDate,
            ProposalNumber = proposalNumber,
            Status = PaymentProposalStatus.Draft
        };
    }

    public void AddLine(Guid invoiceId, Guid supplierId, decimal amount, string? reference = null)
    {
        if (Status != PaymentProposalStatus.Draft)
            throw new InvalidOperationException($"Cannot add lines to proposal in status {Status}.");
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive.");

        _lines.Add(new PaymentProposalLine
        {
            Id = Guid.NewGuid(),
            PaymentProposalId = Id,
            InvoiceId = invoiceId,
            SupplierId = supplierId,
            Amount = amount,
            Reference = reference
        });
    }

    /// <summary>
    /// موافقة CFO — شرط لتوليد ISO 20022 XML
    /// </summary>
    public void Approve(Guid approvedByUserId)
    {
        if (Status != PaymentProposalStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot approve proposal in status {Status}.");
        if (!_lines.Any())
            throw new InvalidOperationException("Cannot approve empty payment proposal.");

        Status = PaymentProposalStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
    }

    public void SubmitForApproval()
    {
        if (Status != PaymentProposalStatus.Draft)
            throw new InvalidOperationException($"Cannot submit proposal in status {Status}.");
        if (!_lines.Any())
            throw new InvalidOperationException("Cannot submit empty payment proposal.");

        Status = PaymentProposalStatus.PendingApproval;
    }

    public void Reject(Guid rejectedByUserId, string reason)
    {
        if (Status != PaymentProposalStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot reject proposal in status {Status}.");

        Status = PaymentProposalStatus.Rejected;
        RejectionReason = reason;
    }

    public void MarkAsExecuted()
    {
        if (Status != PaymentProposalStatus.Approved)
            throw new InvalidOperationException("Only approved proposals can be executed.");
        Status = PaymentProposalStatus.Executed;
    }
}

public class PaymentProposalLine : BaseEntity
{
    public Guid Id { get; set; }
    public Guid PaymentProposalId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid SupplierId { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}

public enum PaymentProposalStatus
{
    Draft = 0,
    PendingApproval = 1,
    Approved = 2,
    Rejected = 3,
    Executed = 4
}
