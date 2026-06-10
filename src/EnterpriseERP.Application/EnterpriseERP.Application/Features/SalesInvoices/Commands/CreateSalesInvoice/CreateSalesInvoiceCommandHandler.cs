using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.SalesInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Accounting;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Sales;
using MediatR;

namespace EnterpriseERP.Application.Features.SalesInvoices.Commands.CreateSalesInvoice;

public class CreateSalesInvoiceCommandHandler : IRequestHandler<CreateSalesInvoiceCommand, SalesInvoiceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountingPostingService _accountingPostingService;
    private readonly IInventoryPostingService _inventoryPostingService;

    public CreateSalesInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountingPostingService accountingPostingService,
        IInventoryPostingService inventoryPostingService)
    {
        _unitOfWork = unitOfWork;
        _accountingPostingService = accountingPostingService;
        _inventoryPostingService = inventoryPostingService;
    }

    public async Task<SalesInvoiceDto> Handle(CreateSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var lines = new List<SalesInvoiceLine>();
            decimal subTotal = 0;
            decimal totalDiscount = 0;
            decimal totalTax = 0;

            foreach (var lineReq in request.Lines)
            {
                var grossAmount = lineReq.Quantity * lineReq.UnitPrice;
                var discountAmount = Math.Round(grossAmount * (lineReq.DiscountPercent / 100m), 4);
                var netBeforeTax = grossAmount - discountAmount;
                var taxAmount = Math.Round(netBeforeTax * (lineReq.TaxPercent / 100m), 4);
                var lineTotal = netBeforeTax + taxAmount;

                lines.Add(SalesInvoiceLine.Create(
                    salesInvoiceId: Guid.Empty,
                    itemId: lineReq.ItemId,
                    itemName: "Item Name", // Should probably fetch from DB but for now mock or fetch
                    quantity: lineReq.Quantity,
                    unitPrice: lineReq.UnitPrice,
                    discountPercent: lineReq.DiscountPercent,
                    taxPercent: lineReq.TaxPercent,
                    warehouseId: request.WarehouseId
                ));

                subTotal += grossAmount;
                totalDiscount += discountAmount;
                totalTax += taxAmount;
            }

            var totalAmount = subTotal - totalDiscount + totalTax;
            var remaining = totalAmount - request.PaidAmount;

            var invoiceNumber = $"SI-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

            var invoice = new SalesInvoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                InvoiceDate = request.InvoiceDate,
                CustomerId = request.CustomerId,
                BranchId = request.BranchId,
                WarehouseId = request.WarehouseId,
                DeliveryNoteId = request.DeliveryNoteId,
                SubTotal = subTotal,
                DiscountAmount = totalDiscount,
                TaxAmount = totalTax,
                TotalAmount = totalAmount,
                PaidAmount = request.PaidAmount,
                RemainingAmount = remaining,
                Notes = request.Notes,
                Status = InvoiceStatus.Draft, // Created as Draft
                Lines = lines
            };

            // Enforce O2C Fulfillment Gate: Sales Invoice must have a Delivery Note
            if (invoice.DeliveryNoteId == null)
            {
                // We allow creating draft invoices, but posting to GL will be blocked by AccountingPostingService
                invoice.Status = InvoiceStatus.Draft;
                invoice.Notes += " | Awaiting Delivery Note link for fulfillment.";
            }
            else
            {
                // If it has a DN, we can approve it and post immediately
                invoice.Status = InvoiceStatus.Approved;
                await _accountingPostingService.PostSalesInvoiceAsync(invoice, cancellationToken);
            }

            await _unitOfWork.Repository<SalesInvoice>().AddAsync(invoice);

            // Add Domain Event
            invoice.AddDomainEvent(new EnterpriseERP.Application.Features.SalesInvoices.Events.SalesInvoiceCreatedEvent(
                invoice.Id, invoice.CustomerId, invoice.TotalAmount, invoice.InvoiceNumber));

            await _unitOfWork.CommitTransactionAsync();

            return new SalesInvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                CustomerId = invoice.CustomerId,
                BranchId = invoice.BranchId,
                WarehouseId = invoice.WarehouseId,
                SubTotal = invoice.SubTotal,
                DiscountAmount = invoice.DiscountAmount,
                TaxAmount = invoice.TaxAmount,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.RemainingAmount,
                Status = invoice.Status.ToString(),
                Notes = invoice.Notes,
                Lines = lines.Select(l => new SalesInvoiceLineDto
                {
                    Id = l.Id,
                    ItemId = l.ItemId,
                    Quantity = l.Quantity,
                    UnitPrice = l.UnitPrice,
                    DiscountPercent = l.DiscountPercent,
                    DiscountAmount = l.DiscountAmount,
                    TaxPercent = l.TaxPercent,
                    TaxAmount = l.TaxAmount,
                    LineTotal = l.LineTotal
                }).ToList()
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
