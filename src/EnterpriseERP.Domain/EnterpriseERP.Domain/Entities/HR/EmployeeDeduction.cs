using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.HR;

public enum DeductionType
{
    Absence = 1,        // غياب
    Advance = 2,        // سلفة
    SocialInsurance = 3, // تأمينات اجتماعية
    Tax = 4,            // ضريبة دخل
    Penalty = 5,        // جزاء / غرامة
    Other = 6           // أخرى
}

/// <summary>
/// استقطاع خاص بموظف لشهر معين، يُطبّق عند تشغيل مسيّر الرواتب
/// </summary>
public class EmployeeDeduction : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DeductionType DeductionType { get; set; }
    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>الشهر والسنة التي ينطبق عليها الاستقطاع</summary>
    public int Year { get; set; }
    public int Month { get; set; }

    /// <summary>هل تم تطبيق هذا الاستقطاع في مسيّر رواتب مغلق؟</summary>
    public bool IsApplied { get; set; } = false;
    public Guid? PayrollTransactionId { get; set; }
}
