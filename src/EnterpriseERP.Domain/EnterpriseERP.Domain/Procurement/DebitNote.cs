using System;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Procurement;

public class DebitNote : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public string NoteNumber { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    
    public Guid SupplierId { get; set; }
    public Guid? PurchaseReturnId { get; set; }
    
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    
    public DebitNoteStatus Status { get; set; }

    public static DebitNote Create(Guid purchaseReturnId, Guid supplierId, decimal amount, string reference)
    {
        return new DebitNote
        {
            Id = Guid.NewGuid(),
            PurchaseReturnId = purchaseReturnId,
            SupplierId = supplierId,
            Amount = amount,
            Reference = reference,
            Status = DebitNoteStatus.Draft
        };
    }
}

public enum DebitNoteStatus
{
    Draft = 0,
    Approved = 1,
    Cancelled = 2
}
