namespace EnterpriseERP.Domain.Enums;

/// <summary>
/// كل الأحداث المحاسبية في النظام — مستمدة من Blueprint Section 4
/// </summary>
public enum PostingKey
{
    // ── Order-to-Cash ────────────────────────────────────────
    SALES_REVENUE           = 100,  // Dr: AR / Cr: هذا الحساب
    AR_RECEIVABLE           = 101,  // Dr: هذا الحساب / Cr: Revenue
    SALES_DISCOUNT          = 102,
    SALES_TAX_PAYABLE       = 103,
    SALES_RETURN            = 104,

    // ── Cost of Goods Sold ────────────────────────────────────
    COGS                    = 200,  // Dr: هذا الحساب / Cr: Inventory
    INVENTORY_ASSET         = 201,  // الأصل المخزني

    // ── Procure-to-Pay ────────────────────────────────────────
    AP_PAYABLE              = 300,  // Cr: هذا الحساب
    PURCHASE_EXPENSE        = 301,
    GRNI_ACCRUAL            = 302,  // Goods Received Not Invoiced
    AP_ACCRUED_LIABILITY    = 303,
    PURCHASE_PRICE_VARIANCE = 304,

    // ── Treasury ──────────────────────────────────────────────
    CASH_ACCOUNT            = 400,
    BANK_ACCOUNT            = 401,
    UNDEPOSITED_FUNDS       = 402,

    // ── Fixed Assets ──────────────────────────────────────────
    FIXED_ASSET             = 500,
    ACCUMULATED_DEPRECIATION = 501,
    DEPRECIATION_EXPENSE    = 502,
    ASSET_DISPOSAL_GAIN     = 503,
    ASSET_DISPOSAL_LOSS     = 504,

    // ── HR & Payroll ──────────────────────────────────────────
    SALARY_EXPENSE          = 600,
    PAYROLL_PAYABLE         = 601,
    EMPLOYEE_WITHHOLDING    = 602,
    SOCIAL_INSURANCE_EXPENSE = 603,
    SOCIAL_INSURANCE_PAYABLE = 604,

    // ── FX ────────────────────────────────────────────────────
    FX_GAIN                 = 700,
    FX_LOSS                 = 701,

    // ── Manufacturing ─────────────────────────────────────────
    WIP_INVENTORY           = 800,
    FINISHED_GOODS          = 801,
    MANUFACTURING_VARIANCE  = 802,

    // ── Tax ───────────────────────────────────────────────────
    VAT_OUTPUT              = 900,
    VAT_INPUT               = 901,
    WITHHOLDING_TAX         = 902,

    // ── Year-End Closing ──────────────────────────────────────
    INCOME_SUMMARY          = 1000,
    RETAINED_EARNINGS       = 1001,

    // ── Consolidation & Intercompany ──────────────────────────
    INTERCOMPANY_DUE_TO     = 1100,
    INTERCOMPANY_DUE_FROM   = 1101,
}
