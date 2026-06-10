using System.Xml.Linq;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Common.Interfaces;
using EnterpriseERP.Domain.Treasury;
using EnterpriseERP.Domain.Entities.Purchasing;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.Treasury.Commands.CreatePaymentProposal
{

/// <summary>
/// دفعة مدفوعات مجمعة — يجمع الفواتير المستحقة + موافقة CFO
/// Blueprint Section 1.4 — Payment Proposal
/// </summary>
public record CreatePaymentProposalCommand : IRequest<CreatePaymentProposalResult>
{
    public Guid CompanyId { get; init; }
    public Guid BankAccountId { get; init; }
    public DateTime ProposalDate { get; init; }
    public DateTime? PaymentDueBefore { get; init; }  // جلب الفواتير المستحقة قبل هذا التاريخ
    public Guid? SupplierId { get; init; }             // null = كل الموردين
}

public class CreatePaymentProposalCommandHandler : IRequestHandler<CreatePaymentProposalCommand, CreatePaymentProposalResult>
{
    private readonly IAppDbContext _context;
    private readonly IOrderNumberGenerator _numberingService;

    public CreatePaymentProposalCommandHandler(
        IAppDbContext context,
        IOrderNumberGenerator numberingService)
    {
        _context = context;
        _numberingService = numberingService;
    }

    public async Task<CreatePaymentProposalResult> Handle(
        CreatePaymentProposalCommand command,
        CancellationToken cancellationToken)
    {
        var dueDate = command.PaymentDueBefore ?? command.ProposalDate.AddDays(7);

        // جلب الفواتير المستحقة القابلة للدفع (Matched فقط)
        var dueInvoicesQuery = _context.PurchaseInvoices
            .Where(i => i.CompanyId == command.CompanyId
                     && i.DueDate <= dueDate
                     && i.RemainingAmount > 0
                     && i.MatchStatus != InvoiceMatchStatus.Exception
                     && i.Status != PurchaseInvoiceStatus.Cancelled
                     && i.Status != PurchaseInvoiceStatus.OnHold);

        if (command.SupplierId.HasValue)
            dueInvoicesQuery = dueInvoicesQuery.Where(i => i.SupplierId == command.SupplierId.Value);

        var dueInvoices = await dueInvoicesQuery
            .OrderBy(i => i.DueDate)
            .ToListAsync(cancellationToken);

        if (!dueInvoices.Any())
            return new CreatePaymentProposalResult
            {
                TotalInvoices = 0,
                TotalAmount = 0,
                Message = "No due invoices found matching criteria"
            };

        var proposalNumber = await _numberingService.GenerateAsync("PP", command.CompanyId, Guid.Empty, cancellationToken);

        var proposal = PaymentProposal.Create(
            command.CompanyId,
            command.BankAccountId,
            command.ProposalDate,
            command.ProposalDate.AddDays(1),
            proposalNumber);

        foreach (var inv in dueInvoices)
        {
            proposal.AddLine(inv.Id, inv.SupplierId, inv.RemainingAmount);
        }

        proposal.SubmitForApproval();

        _context.PaymentProposals.Add(proposal);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePaymentProposalResult
        {
            ProposalId = proposal.Id,
            ProposalNumber = proposalNumber,
            TotalInvoices = dueInvoices.Count,
            TotalAmount = dueInvoices.Sum(i => i.RemainingAmount),
            Status = PaymentProposalStatus.PendingApproval,
            Message = $"Payment proposal created. Awaiting CFO approval before bank transmission."
        };
    }
}

public class CreatePaymentProposalResult
{
    public Guid? ProposalId { get; init; }
    public string? ProposalNumber { get; init; }
    public int TotalInvoices { get; init; }
    public decimal TotalAmount { get; init; }
    public PaymentProposalStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
}

public enum PaymentProposalStatus
{
    PendingApproval = 0,
    ApprovedByCFO = 1,
    SentToBank = 2,
    Processed = 3,
    Rejected = 4
}
}

// ═══════════════════════════════════════════════════════════════
// ISO 20022 pain.001 Generator
// توليد XML لأوامر الدفع وفق المعيار الدولي
// ═══════════════════════════════════════════════════════════════

namespace EnterpriseERP.Application.Services.Iso20022
{

public class Pain001Generator
{
    /// <summary>
    /// توليد ملف XML وفق ISO 20022 pain.001.001.09
    /// يُستخدم لإرسال أوامر الدفع للبنك
    /// </summary>
    public string GeneratePain001(Pain001Request request)
    {
        var msgId = $"MSG-{request.ProposalNumber}-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var creationDateTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement("Document",
                new XAttribute("xmlns", "urn:iso:std:iso:20022:tech:xsd:pain.001.001.09"),
                new XElement("CstmrCdtTrfInitn",

                    // Group Header
                    new XElement("GrpHdr",
                        new XElement("MsgId", msgId),
                        new XElement("CreDtTm", creationDateTime),
                        new XElement("NbOfTxs", request.Transactions.Count.ToString()),
                        new XElement("CtrlSum", request.Transactions.Sum(t => t.Amount).ToString("F2")),
                        new XElement("InitgPty",
                            new XElement("Nm", request.InitiatingPartyName)
                        )
                    ),

                    // Payment Information
                    new XElement("PmtInf",
                        new XElement("PmtInfId", $"PMT-{request.ProposalNumber}"),
                        new XElement("PmtMtd", "TRF"),
                        new XElement("NbOfTxs", request.Transactions.Count.ToString()),
                        new XElement("CtrlSum", request.Transactions.Sum(t => t.Amount).ToString("F2")),
                        new XElement("PmtTpInf",
                            new XElement("SvcLvl",
                                new XElement("Cd", "SEPA")
                            )
                        ),
                        new XElement("ReqdExctnDt",
                            new XElement("Dt", request.RequestedExecutionDate.ToString("yyyy-MM-dd"))
                        ),
                        new XElement("Dbtr",
                            new XElement("Nm", request.DebtorName)
                        ),
                        new XElement("DbtrAcct",
                            new XElement("Id",
                                new XElement("IBAN", request.DebtorIBAN)
                            )
                        ),
                        new XElement("DbtrAgt",
                            new XElement("FinInstnId",
                                new XElement("BICFI", request.DebtorBIC)
                            )
                        ),

                        // Credit Transfer Transactions
                        request.Transactions.Select((tx, idx) =>
                            new XElement("CdtTrfTxInf",
                                new XElement("PmtId",
                                    new XElement("InstrId", $"INSTR-{idx + 1:D4}"),
                                    new XElement("EndToEndId", tx.EndToEndId)
                                ),
                                new XElement("Amt",
                                    new XElement("InstdAmt",
                                        new XAttribute("Ccy", tx.Currency),
                                        tx.Amount.ToString("F2")
                                    )
                                ),
                                new XElement("CdtrAgt",
                                    new XElement("FinInstnId",
                                        new XElement("BICFI", tx.CreditorBIC)
                                    )
                                ),
                                new XElement("Cdtr",
                                    new XElement("Nm", tx.CreditorName)
                                ),
                                new XElement("CdtrAcct",
                                    new XElement("Id",
                                        new XElement("IBAN", tx.CreditorIBAN)
                                    )
                                ),
                                new XElement("RmtInf",
                                    new XElement("Ustrd", tx.RemittanceInfo)
                                )
                            )
                        )
                    )
                )
            )
        );

        return doc.ToString();
    }
}

public class Pain001Request
{
    public string ProposalNumber { get; init; } = default!;
    public string InitiatingPartyName { get; init; } = default!;
    public string DebtorName { get; init; } = default!;
    public string DebtorIBAN { get; init; } = default!;
    public string DebtorBIC { get; init; } = default!;
    public DateTime RequestedExecutionDate { get; init; }
    public List<Pain001Transaction> Transactions { get; init; } = new();
}

public class Pain001Transaction
{
    public string EndToEndId { get; init; } = default!;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "USD";
    public string CreditorName { get; init; } = default!;
    public string CreditorIBAN { get; init; } = default!;
    public string CreditorBIC { get; init; } = default!;
    public string RemittanceInfo { get; init; } = default!;
}
}
