namespace EnterpriseERP.Application.Features.SalesReturns.DTOs;

public class CreditNoteDto
{
    public Guid Id { get; set; }
    public string NoteNumber { get; set; } = string.Empty;
    public DateTime NoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? SalesReturnId { get; set; }
    public string? ReturnNumber { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
}
