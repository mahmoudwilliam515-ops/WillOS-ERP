namespace EnterpriseERP.Domain.Entities.Sales;

public enum EInvoiceAuthority
{
    None = 0,
    Zatca = 1,
    Eta = 2
}

public enum EInvoiceSubmissionStatus
{
    NotSubmitted = 0,
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}
