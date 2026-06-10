using EnterpriseERP.SharedKernel.Common;
using EnterpriseERP.Domain.Sales;

namespace EnterpriseERP.Domain.Entities.Sales;

public class CreditNote : AuditableEntity, IAggregateRoot, ISoftDelete
{
    public string NoteNumber { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    
    public Guid CustomerId { get; set; }
    public Guid? SalesReturnId { get; set; } // If generated from a return
    
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    
    // 0 = Draft, 1 = Approved
    public int Status { get; set; } 
    
    public SalesReturn? SalesReturn { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
