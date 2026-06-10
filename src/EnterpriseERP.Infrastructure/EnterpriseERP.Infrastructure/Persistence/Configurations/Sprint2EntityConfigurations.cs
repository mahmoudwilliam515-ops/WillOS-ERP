using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Treasury;
using EnterpriseERP.Domain.Reporting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseERP.Infrastructure.Persistence.Configurations;

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// GoodsReceiptNote Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class GoodsReceiptNoteConfiguration : IEntityTypeConfiguration<GoodsReceiptNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNote> builder)
    {
        builder.ToTable("GoodsReceiptNotes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.GRNNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.DeliveryNoteReference)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Ø¹Ù„Ø§Ù‚Ø© Ù…Ø¹ Lines
        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.GoodsReceiptNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes â€” TenantId + Date + Status
        builder.HasIndex(e => new { e.CompanyId, e.ReceiptDate, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.PurchaseOrderId });
        builder.HasIndex(e => new { e.CompanyId, e.GRNNumber }).IsUnique();

        // Concurrency
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}

public class GoodsReceiptNoteLineConfiguration : IEntityTypeConfiguration<GoodsReceiptNoteLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNoteLine> builder)
    {
        builder.ToTable("GoodsReceiptNoteLines");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReceivedQuantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.UnitCost)
            .HasColumnType("decimal(18,4)");

        builder.Property(e => e.BatchNumber)
            .HasMaxLength(50);

        builder.Property(e => e.MatchStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(e => new { e.GoodsReceiptNoteId });
        builder.HasIndex(e => new { e.PurchaseOrderLineId });
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// MatchToleranceRule Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class MatchToleranceRuleConfiguration : IEntityTypeConfiguration<MatchToleranceRule>
{
    public void Configure(EntityTypeBuilder<MatchToleranceRule> builder)
    {
        builder.ToTable("MatchToleranceRules");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PriceTolerancePercent)
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.QuantityTolerancePercent)
            .HasColumnType("decimal(5,2)");

        builder.HasIndex(e => new { e.CompanyId, e.ItemId }).IsUnique();
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// PurchaseReturn Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("PurchaseReturns");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReturnNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(e => e.Reason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.ReturnDate, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.ReturnNumber }).IsUnique();
    }
}

public class PurchaseReturnLineConfiguration : IEntityTypeConfiguration<PurchaseReturnLine>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnLine> builder)
    {
        builder.ToTable("PurchaseReturnLines");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReturnQuantity).HasColumnType("decimal(18,4)");
        builder.Property(e => e.UnitCost).HasColumnType("decimal(18,4)");
    }
}

public class DebitNoteConfiguration : IEntityTypeConfiguration<DebitNote>
{
    public void Configure(EntityTypeBuilder<DebitNote> builder)
    {
        builder.ToTable("DebitNotes");
        builder.HasKey(e => e.Id);

                builder.Property(e => e.NoteNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => new { e.CompanyId, e.NoteNumber }).IsUnique();
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// SalesOrder Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(e => e.Id);

                builder.Property(e => e.OrderNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                        builder.Property(e => e.CustomerReference).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.SalesOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.OrderDate, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.CustomerId });
        builder.HasIndex(e => new { e.CompanyId, e.OrderNumber }).IsUnique();

        builder.Property<byte[]>("RowVersion").IsRowVersion().IsConcurrencyToken();
    }
}

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderedQuantity).HasColumnType("decimal(18,4)");
        builder.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");

        builder.HasIndex(e => new { e.SalesOrderId });
        builder.HasIndex(e => new { e.ItemId });
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// DeliveryNote Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class DeliveryNoteConfiguration : IEntityTypeConfiguration<DeliveryNote>
{
    public void Configure(EntityTypeBuilder<DeliveryNote> builder)
    {
        builder.ToTable("DeliveryNotes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.DeliveryNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.TrackingNumber).HasMaxLength(100);
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.DeliveryNoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.DeliveryDate, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.SalesOrderId });
        builder.HasIndex(e => new { e.CompanyId, e.DeliveryNumber }).IsUnique();
    }
}

public class DeliveryNoteLineConfiguration : IEntityTypeConfiguration<DeliveryNoteLine>
{
    public void Configure(EntityTypeBuilder<DeliveryNoteLine> builder)
    {
        builder.ToTable("DeliveryNoteLines");
        builder.HasKey(e => e.Id);

    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// SalesReturn Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ToTable("SalesReturns");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReturnNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Reason).IsRequired().HasMaxLength(500);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.ReturnDate, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.ReturnNumber }).IsUnique();
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// ReservationEntry Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class ReservationEntryConfiguration : IEntityTypeConfiguration<ReservationEntry>
{
    public void Configure(EntityTypeBuilder<ReservationEntry> builder)
    {
        builder.ToTable("ReservationEntries");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ReservedQuantity).HasColumnType("decimal(18,4)");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => new { e.CompanyId, e.ItemId, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.SalesOrderId });
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// PaymentProposal Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class PaymentProposalConfiguration : IEntityTypeConfiguration<PaymentProposal>
{
    public void Configure(EntityTypeBuilder<PaymentProposal> builder)
    {
        builder.ToTable("PaymentProposals");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProposalNumber).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.RejectionReason).HasMaxLength(500);

        builder.HasMany(e => e.Lines)
            .WithOne()
            .HasForeignKey(l => l.PaymentProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.Status });
        builder.HasIndex(e => new { e.CompanyId, e.ProposalNumber }).IsUnique();
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// PeriodCloseChecklist Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class PeriodCloseChecklistConfiguration : IEntityTypeConfiguration<PeriodCloseChecklist>
{
    public void Configure(EntityTypeBuilder<PeriodCloseChecklist> builder)
    {
        builder.ToTable("PeriodCloseChecklists");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PeriodName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(e => e.Steps)
            .WithOne()
            .HasForeignKey(i => i.PeriodCloseChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.CompanyId, e.FiscalPeriodId }).IsUnique();
    }
}

public class PeriodCloseChecklistStepConfiguration : IEntityTypeConfiguration<PeriodCloseChecklistStep>
{
    public void Configure(EntityTypeBuilder<PeriodCloseChecklistStep> builder)
    {
        builder.ToTable("PeriodCloseChecklistSteps");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(30);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ResponsibleRole).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(500);
    }
}

// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•
// UnmatchedBankTransaction Configuration
// â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•

public class UnmatchedBankTransactionConfiguration : IEntityTypeConfiguration<EnterpriseERP.Application.Services.BankStatement.UnmatchedBankTransaction>
{
    public void Configure(EntityTypeBuilder<EnterpriseERP.Application.Services.BankStatement.UnmatchedBankTransaction> builder)
    {
        builder.ToTable("UnmatchedBankTransactions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Amount).HasColumnType("decimal(18,2)");
        builder.Property(e => e.Reference).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => new { e.CompanyId, e.BankAccountId, e.IsResolved });
        builder.HasIndex(e => new { e.CompanyId, e.ValueDate });
    }
}




