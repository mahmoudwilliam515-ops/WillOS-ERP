namespace EnterpriseERP.Application.Features.SalesInvoices.DTOs;

public class EInvoicePreviewDto
{
    public Guid SalesInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Authority { get; set; } = string.Empty;
    public string SubmissionStatus { get; set; } = string.Empty;
    public string? SubmissionUuid { get; set; }
    public string? QrPayloadBase64 { get; set; }
    public string? XmlHash { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string XmlPreview { get; set; } = string.Empty;
    public bool CanSubmit { get; set; }
    public string? BlockReason { get; set; }
}

public class EInvoiceSubmissionResultDto
{
    public bool Success { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string SubmissionStatus { get; set; } = string.Empty;
    public string? SubmissionUuid { get; set; }
    public string? QrPayloadBase64 { get; set; }
    public string Message { get; set; } = string.Empty;
}
