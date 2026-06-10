using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using EnterpriseERP.Domain.Entities.Settings;
using EnterpriseERP.Domain.Entities.FixedAssets;
using EnterpriseERP.Domain.Entities.HR;
using EnterpriseERP.Domain.Entities.Treasury;
using EnterpriseERP.Domain.Entities.SaaS;
using EnterpriseERP.Domain.Entities.CRM;
using EnterpriseERP.Domain.Entities.Audit;
using EnterpriseERP.Domain.Entities.Manufacturing;
using EnterpriseERP.Domain.Entities.Workflow;
using EnterpriseERP.Domain.Entities.Projects;
using EnterpriseERP.Domain.Entities.Quality;
using EnterpriseERP.Domain.Entities.Taxation;
using EnterpriseERP.Domain.Entities.EInvoice;
using EnterpriseERP.Domain.Entities.Reporting;
using EnterpriseERP.Domain.Entities.Migration;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Sales;
using EnterpriseERP.Domain.Treasury;
using EnterpriseERP.Domain.Reporting;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Branch> Branches { get; }
    DbSet<Company> Companies { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Item> Items { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<InventoryTransaction> InventoryTransactions { get; }
    DbSet<InventoryBalance> InventoryBalances { get; }
    DbSet<InventoryFIFOLayer> InventoryFIFOLayers { get; }
    
    // Procurement
    DbSet<EnterpriseERP.Domain.Procurement.GoodsReceiptNote> GoodsReceiptNotes { get; }
    DbSet<GoodsReceiptNoteLine> GoodsReceiptNoteLines { get; }
    DbSet<MatchToleranceRule> MatchToleranceRules { get; }
    DbSet<EnterpriseERP.Domain.Procurement.PurchaseReturn> PurchaseReturns { get; }
    DbSet<EnterpriseERP.Domain.Procurement.DebitNote> DebitNotes { get; }

    // Sales
    DbSet<SalesInvoice> SalesInvoices { get; }
    DbSet<EnterpriseERP.Domain.Sales.SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderLine> SalesOrderLines { get; }
    DbSet<ReservationEntry> ReservationEntries { get; }
    DbSet<EnterpriseERP.Domain.Sales.DeliveryNote> DeliveryNotes { get; }
    DbSet<DeliveryNoteLine> DeliveryNoteLines { get; }
    DbSet<EnterpriseERP.Domain.Sales.SalesReturn> SalesReturns { get; }
    DbSet<DunningNotice> DunningNotices { get; }

    // Treasury
    DbSet<EnterpriseERP.Domain.Treasury.PaymentProposal> PaymentProposals { get; }
    DbSet<PaymentProposalLine> PaymentProposalLines { get; }
    DbSet<EnterpriseERP.Domain.Entities.Treasury.InvoicePayment> InvoicePayments { get; }
    DbSet<EnterpriseERP.Application.Services.BankStatement.UnmatchedBankTransaction> UnmatchedBankTransactions { get; }

    // Reporting
    DbSet<EnterpriseERP.Domain.Reporting.PeriodCloseChecklist> PeriodCloseChecklists { get; }
    DbSet<PeriodCloseChecklistStep> PeriodCloseChecklistItems { get; }

    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseInvoice> PurchaseInvoices { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<GeneralLedgerEntry> GeneralLedgerEntries { get; }
    DbSet<Account> Accounts { get; }
    DbSet<AccountingPeriod> AccountingPeriods { get; }
    DbSet<AccountMapping> AccountMappings { get; }
    DbSet<ReceiptVoucher> ReceiptVouchers { get; }
    DbSet<PaymentVoucher> PaymentVouchers { get; }
    DbSet<EnterpriseERP.Domain.Entities.Treasury.BankAccount> BankAccounts { get; }
    DbSet<ProductionOrder> ProductionOrders { get; }
    DbSet<EnterpriseERP.Domain.Entities.FixedAssets.FixedAsset> FixedAssets { get; }
    
    // Add other DbSets as needed for the application
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
