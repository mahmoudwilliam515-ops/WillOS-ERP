using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Treasury;

/// <summary>
/// حالة سندات القبض والصرف والتحويل البنكي
/// مُعرَّف هنا مرة واحدة فقط — لا تكرار في ملفات أخرى
/// </summary>
public enum VoucherStatus
{
    Draft = 0,
    Approved = 1,
    Cancelled = 2
}
