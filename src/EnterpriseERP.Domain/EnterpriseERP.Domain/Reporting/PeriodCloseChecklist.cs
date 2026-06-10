using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Reporting;

/// <summary>
/// قائمة مراجعة إغلاق الفترة المحاسبية
/// Blueprint Section 1.3 — Period Close Workflow
/// يجب على CFO الموافقة قبل قفل الفترة
/// </summary>
public class PeriodCloseChecklist : AuditableEntity
{
    public Guid Id { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid FiscalPeriodId { get; private set; }
    public string PeriodName { get; private set; } = default!;
    public PeriodCloseStatus Status { get; private set; }
    public Guid? FinalApprovedByUserId { get; private set; }
    public DateTime? FinalApprovedAt { get; private set; }

    private readonly List<PeriodCloseChecklistStep> _steps = new();
    public IReadOnlyCollection<PeriodCloseChecklistStep> Steps => _steps.AsReadOnly();

    private PeriodCloseChecklist() { }

    public static PeriodCloseChecklist Create(
        Guid companyId,
        Guid fiscalPeriodId,
        string periodName)
    {
        var checklist = new PeriodCloseChecklist
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            FiscalPeriodId = fiscalPeriodId,
            PeriodName = periodName,
            Status = PeriodCloseStatus.InProgress
        };

        // إضافة خطوات الإغلاق المعيارية
        checklist.AddDefaultSteps();
        return checklist;
    }

    private void AddDefaultSteps()
    {
        var defaultSteps = new[]
        {
            ("AR_RECONCILE",  "تسوية AR Subledger مع GL Control Account",   "AccountsReceivable"),
            ("AP_RECONCILE",  "تسوية AP Subledger مع GL Control Account",   "AccountsPayable"),
            ("BANK_RECON",    "تسوية كشف الحساب البنكي",                    "Treasury"),
            ("GRNI_CLEAR",    "تصفية حساب GRNI Accrual",                   "Procurement"),
            ("DEPRECIATION",  "ترحيل قيد الإهلاك الشهري",                  "FixedAssets"),
            ("PREPAID",       "تسوية المصروفات المدفوعة مقدماً",            "GeneralLedger"),
            ("ACCRUALS",      "ترحيل الاستحقاقات الشهرية",                  "GeneralLedger"),
            ("INTERCOMPANY",  "تسوية معاملات ما بين الشركات",              "Consolidation"),
            ("TRIAL_BALANCE", "مراجعة ميزان المراجعة",                      "Reporting"),
            ("CFO_APPROVAL",  "موافقة CFO على الإغلاق النهائي",             "CFO")
        };

        int order = 1;
        foreach (var (code, description, responsibleRole) in defaultSteps)
        {
            _steps.Add(PeriodCloseChecklistStep.Create(Id, code, description, responsibleRole, order++));
        }
    }

    public void SignOffItem(string itemCode, Guid userId, string? notes = null)
    {
        var item = _steps.FirstOrDefault(i => i.Code == itemCode)
            ?? throw new InvalidOperationException($"Checklist item '{itemCode}' not found.");

        if (Status == PeriodCloseStatus.Locked)
            throw new InvalidOperationException("Cannot modify a locked period checklist.");

        item.SignOff(userId, notes);
    }

    /// <summary>
    /// قفل الفترة — يتطلب موافقة CFO (آخر خطوة)
    /// </summary>
    public void LockPeriod(Guid cfoUserId)
    {
        var pendingItems = _steps.Where(i => !i.IsSignedOff).ToList();
        if (pendingItems.Any())
        {
            var pendingNames = string.Join(", ", pendingItems.Select(i => i.Code));
            throw new InvalidOperationException(
                $"Cannot lock period. The following items are not signed off: {pendingNames}");
        }

        Status = PeriodCloseStatus.Locked;
        FinalApprovedByUserId = cfoUserId;
        FinalApprovedAt = DateTime.UtcNow;
    }

    public bool AllItemsSignedOff => _steps.All(i => i.IsSignedOff);
}

public class PeriodCloseChecklistStep : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid PeriodCloseChecklistId { get; private set; }
    public string Code { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public string ResponsibleRole { get; private set; } = default!;
    public int DisplayOrder { get; private set; }
    public bool IsSignedOff { get; private set; }
    public Guid? SignedOffByUserId { get; private set; }
    public DateTime? SignedOffAt { get; private set; }
    public string? Notes { get; private set; }

    private PeriodCloseChecklistStep() { }

    internal static PeriodCloseChecklistStep Create(
        Guid checklistId,
        string code,
        string description,
        string responsibleRole,
        int displayOrder)
    {
        return new PeriodCloseChecklistStep
        {
            Id = Guid.NewGuid(),
            PeriodCloseChecklistId = checklistId,
            Code = code,
            Description = description,
            ResponsibleRole = responsibleRole,
            DisplayOrder = displayOrder,
            IsSignedOff = false
        };
    }

    internal void SignOff(Guid userId, string? notes)
    {
        IsSignedOff = true;
        SignedOffByUserId = userId;
        SignedOffAt = DateTime.UtcNow;
        Notes = notes;
    }
}

public enum PeriodCloseStatus
{
    InProgress = 0,
    Locked = 1
}
