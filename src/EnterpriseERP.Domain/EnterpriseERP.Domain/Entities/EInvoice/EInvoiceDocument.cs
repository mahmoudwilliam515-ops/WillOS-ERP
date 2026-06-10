using EnterpriseERP.SharedKernel.Common;
using System;

namespace EnterpriseERP.Domain.Entities.EInvoice;

public enum EInvoiceProvider
{
    ZATCA, // Saudi Arabia
    ETA    // Egypt
}

public enum EInvoiceStatus
{
    Draft,
    Reported,
    Cleared,
    Rejected,
    Cancelled
}

public class EInvoiceDocument : AuditableEntity, IAggregateRoot
{
    public Guid SourceDocumentId { get; private set; }
    public string SourceDocumentType { get; private set; } = "SalesInvoice";
    public EInvoiceProvider Provider { get; private set; }
    public EInvoiceStatus Status { get; private set; } = EInvoiceStatus.Draft;
    
    // ZATCA / ETA Specific Data
    public string Uuid { get; private set; } = string.Empty;
    public string InvoiceHash { get; private set; } = string.Empty;
    public string CryptographicStamp { get; private set; } = string.Empty;
    public string QrCodeData { get; private set; } = string.Empty;
    public string PreviousInvoiceHash { get; private set; } = string.Empty;
    
    // Authority Response
    public string AuthorityResponseCode { get; private set; } = string.Empty;
    public string AuthorityResponseMessage { get; private set; } = string.Empty;
    public DateTime? SubmittedAt { get; private set; }

    private EInvoiceDocument() { }

    public EInvoiceDocument(Guid sourceDocumentId, string sourceDocumentType, EInvoiceProvider provider, string previousInvoiceHash)
    {
        SourceDocumentId = sourceDocumentId;
        SourceDocumentType = sourceDocumentType;
        Provider = provider;
        PreviousInvoiceHash = previousInvoiceHash;
        Uuid = Guid.NewGuid().ToString();
    }

    public void UpdateStatus(EInvoiceStatus status, string responseCode, string responseMessage)
    {
        Status = status;
        AuthorityResponseCode = responseCode;
        AuthorityResponseMessage = responseMessage;
        SubmittedAt = DateTime.UtcNow;
    }

    public void SetZatcaData(string hash, string cryptographicStamp, string qrCodeData)
    {
        InvoiceHash = hash;
        CryptographicStamp = cryptographicStamp;
        QrCodeData = qrCodeData;
    }
}
