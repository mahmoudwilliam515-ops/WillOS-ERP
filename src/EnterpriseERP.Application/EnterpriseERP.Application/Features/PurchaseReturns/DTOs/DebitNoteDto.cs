namespace EnterpriseERP.Application.Features.PurchaseReturns.DTOs;

public class DebitNoteDto
{
    public Guid Id { get; set; }
    public string NoteNumber { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public Guid? PurchaseReturnId { get; set; }
    public string? ReturnNumber { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
