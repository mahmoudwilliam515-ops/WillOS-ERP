namespace EnterpriseERP.Domain.Enums;

public enum InvoiceSettlementStatus
{
    Outstanding = 0,    // لم يُدفع أي شيء
    PartiallyPaid = 1,  // دُفع جزء
    FullyPaid = 2,      // مُسدَّد بالكامل
    Cancelled = 3       // ملغي
}
