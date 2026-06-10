using System;
using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Sales;

public class DunningNotice : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid CustomerId { get; set; }
    public int Level { get; set; }
    public int DaysOverdue { get; set; }
    public decimal AmountDue { get; set; }
    public DateTime SentAt { get; set; }
    public string Message { get; set; } = default!;
}
