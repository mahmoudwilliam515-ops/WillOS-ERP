using EnterpriseERP.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace EnterpriseERP.Application.Services.BankStatement;

// ═══════════════════════════════════════════════════════════
// Command — استيراد كشف الحساب البنكي (MT940 / CAMT.053)
// ═══════════════════════════════════════════════════════════

public record ImportBankStatementCommand : IRequest<ImportBankStatementResult>
{
    public Guid CompanyId { get; init; }
    public Guid BankAccountId { get; init; }
    public string FileContent { get; init; } = default!;
    public BankStatementFormat Format { get; init; }
}

public enum BankStatementFormat
{
    MT940 = 0,
    CAMT053 = 1
}

public record ImportBankStatementResult(
    int TotalTransactions,
    int MatchedTransactions,
    int UnmatchedTransactions,
    decimal OpeningBalance,
    decimal ClosingBalance,
    List<string> Errors);

// ═══════════════════════════════════════════════════════════
// Validator
// ═══════════════════════════════════════════════════════════

public class ImportBankStatementCommandValidator : AbstractValidator<ImportBankStatementCommand>
{
    public ImportBankStatementCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.FileContent).NotEmpty().WithMessage("Bank statement file content is required.");
        RuleFor(x => x.Format).IsInEnum();
    }
}

// ═══════════════════════════════════════════════════════════
// Handler
// ═══════════════════════════════════════════════════════════

public class ImportBankStatementCommandHandler : IRequestHandler<ImportBankStatementCommand, ImportBankStatementResult>
{
    private readonly IAppDbContext _context;
    private readonly IMT940Parser _mt940Parser;
    private readonly ICAMT053Parser _camt053Parser;

    public ImportBankStatementCommandHandler(
        IAppDbContext context,
        IMT940Parser mt940Parser,
        ICAMT053Parser camt053Parser)
    {
        _context = context;
        _mt940Parser = mt940Parser;
        _camt053Parser = camt053Parser;
    }

    public async Task<ImportBankStatementResult> Handle(
        ImportBankStatementCommand command,
        CancellationToken cancellationToken)
    {
        // 1. التحقق من وجود الحساب البنكي
        var bankAccount = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.Id == command.BankAccountId && b.CompanyId == command.CompanyId, cancellationToken)
            ?? throw new InvalidOperationException($"Bank account {command.BankAccountId} not found.");

        // 2. تحليل الملف حسب التنسيق
        List<BankStatementTransaction> transactions;
        decimal openingBalance = 0;
        decimal closingBalance = 0;
        var errors = new List<string>();

        if (command.Format == BankStatementFormat.MT940)
        {
            var parsed = _mt940Parser.Parse(command.FileContent);
            transactions = parsed.Transactions;
            openingBalance = parsed.OpeningBalance;
            closingBalance = parsed.ClosingBalance;
        }
        else
        {
            var parsed = _camt053Parser.Parse(command.FileContent);
            transactions = parsed.Transactions;
            openingBalance = parsed.OpeningBalance;
            closingBalance = parsed.ClosingBalance;
        }

        // 3. مطابقة الحركات مع السجلات الموجودة
        int matched = 0;
        int unmatched = 0;

        foreach (var txn in transactions)
        {
            try
            {
                // محاولة المطابقة مع إيصالات/مدفوعات موجودة
                bool isMatched = false;

                if (txn.Amount > 0) // قبض
                {
                    var receipt = await _context.ReceiptVouchers
                        .FirstOrDefaultAsync(r =>
                            r.CompanyId == command.CompanyId &&
                            r.BankAccountId == command.BankAccountId &&
                            r.Amount == txn.Amount &&
                            r.ReceiptDate.Date == txn.ValueDate.Date &&
                            r.BankStatementReference == null,
                            cancellationToken);

                    if (receipt != null)
                    {
                        receipt.SetBankStatementReference(txn.Reference);
                        isMatched = true;
                        matched++;
                    }
                }
                else // دفع
                {
                    var payment = await _context.PaymentVouchers
                        .FirstOrDefaultAsync(p =>
                            p.CompanyId == command.CompanyId &&
                            p.BankAccountId == command.BankAccountId &&
                            p.Amount == Math.Abs(txn.Amount) &&
                            p.PaymentDate.Date == txn.ValueDate.Date &&
                            p.BankStatementReference == null,
                            cancellationToken);

                    if (payment != null)
                    {
                        payment.SetBankStatementReference(txn.Reference);
                        isMatched = true;
                        matched++;
                    }
                }

                if (!isMatched)
                {
                    unmatched++;
                    // إنشاء حركة غير مطابقة لمراجعة يدوية
                    _context.UnmatchedBankTransactions.Add(new UnmatchedBankTransaction
                    {
                        Id = Guid.NewGuid(),
                        CompanyId = command.CompanyId,
                        BankAccountId = command.BankAccountId,
                        TransactionDate = txn.TransactionDate,
                        ValueDate = txn.ValueDate,
                        Amount = txn.Amount,
                        Reference = txn.Reference,
                        Description = txn.Description,
                        ImportedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing transaction {txn.Reference}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ImportBankStatementResult(
            transactions.Count,
            matched,
            unmatched,
            openingBalance,
            closingBalance,
            errors);
    }
}

// ═══════════════════════════════════════════════════════════
// Parser Interfaces
// ═══════════════════════════════════════════════════════════

public interface IMT940Parser
{
    ParsedBankStatement Parse(string content);
}

public interface ICAMT053Parser
{
    ParsedBankStatement Parse(string content);
}

public record ParsedBankStatement(
    decimal OpeningBalance,
    decimal ClosingBalance,
    List<BankStatementTransaction> Transactions);

public record BankStatementTransaction(
    DateTime TransactionDate,
    DateTime ValueDate,
    decimal Amount,
    string Reference,
    string Description);

// ═══════════════════════════════════════════════════════════
// MT940 Parser Implementation
// ═══════════════════════════════════════════════════════════

public class MT940Parser : IMT940Parser
{
    public ParsedBankStatement Parse(string content)
    {
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var transactions = new List<BankStatementTransaction>();
        decimal openingBalance = 0;
        decimal closingBalance = 0;

        DateTime? txnDate = null;
        decimal? txnAmount = null;
        string txnRef = "";
        string txnDesc = "";

        foreach (var line in lines)
        {
            // :60F: — Opening Balance
            if (line.StartsWith(":60F:") || line.StartsWith(":60M:"))
            {
                openingBalance = ParseMT940Amount(line.Substring(5));
            }
            // :62F: — Closing Balance
            else if (line.StartsWith(":62F:") || line.StartsWith(":62M:"))
            {
                closingBalance = ParseMT940Amount(line.Substring(5));
            }
            // :61: — Statement Line (Transaction)
            else if (line.StartsWith(":61:"))
            {
                var data = line.Substring(4);
                // تنسيق: YYMMDD[MMDD]2a[1!a]15d[//16x][/34x]
                if (data.Length >= 10)
                {
                    try
                    {
                        var dateStr = data.Substring(0, 6);
                        var year = 2000 + int.Parse(dateStr.Substring(0, 2));
                        var month = int.Parse(dateStr.Substring(2, 2));
                        var day = int.Parse(dateStr.Substring(4, 2));
                        txnDate = new DateTime(year, month, day);

                        // D=Debit, C=Credit
                        var dcIdx = 6;
                        if (data.Length > dcIdx && (data[dcIdx] == 'R')) dcIdx++; // RD or RC
                        bool isCredit = dcIdx < data.Length && data[dcIdx] == 'C';
                        dcIdx++;

                        // المبلغ — يبدأ بعد مؤشر D/C
                        var amountMatch = Regex.Match(data.Substring(dcIdx), @"^[A-Z]{0,3}([\d,]+)");
                        if (amountMatch.Success)
                        {
                            var amountStr = amountMatch.Groups[1].Value.Replace(",", ".");
                            txnAmount = decimal.Parse(amountStr, System.Globalization.CultureInfo.InvariantCulture);
                            if (!isCredit) txnAmount = -txnAmount;
                        }
                    }
                    catch
                    {
                        // سطر غير قابل للتحليل — تجاهل
                    }
                }
            }
            // :86: — Information to Account Owner (Description)
            else if (line.StartsWith(":86:"))
            {
                txnDesc = line.Substring(4);
                if (txnDate.HasValue && txnAmount.HasValue)
                {
                    transactions.Add(new BankStatementTransaction(
                        txnDate.Value,
                        txnDate.Value,
                        txnAmount.Value,
                        txnRef,
                        txnDesc));
                    txnDate = null;
                    txnAmount = null;
                    txnRef = "";
                    txnDesc = "";
                }
            }
        }

        return new ParsedBankStatement(openingBalance, closingBalance, transactions);
    }

    private decimal ParseMT940Amount(string data)
    {
        // تنسيق: C/D YYMMDD EUR 1234,56
        try
        {
            bool isDebit = data.Length > 0 && data[0] == 'D';
            var amountMatch = Regex.Match(data, @"([\d,]+)$");
            if (amountMatch.Success)
            {
                var amount = decimal.Parse(
                    amountMatch.Value.Replace(",", "."),
                    System.Globalization.CultureInfo.InvariantCulture);
                return isDebit ? -amount : amount;
            }
        }
        catch { }
        return 0;
    }
}

// ═══════════════════════════════════════════════════════════
// Entity: UnmatchedBankTransaction
// ═══════════════════════════════════════════════════════════

public class UnmatchedBankTransaction
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid BankAccountId { get; set; }
    public DateTime TransactionDate { get; set; }
    public DateTime ValueDate { get; set; }
    public decimal Amount { get; set; }
    public string Reference { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime ImportedAt { get; set; }
    public bool IsResolved { get; set; }
}
