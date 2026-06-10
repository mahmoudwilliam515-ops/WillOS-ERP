using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Application.Features.PurchaseInvoices.DTOs;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Procurement;
using EnterpriseERP.Domain.Exceptions;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseERP.Application.Features.PurchaseInvoices.Queries.GetThreeWayMatchPreview;

public record GetThreeWayMatchPreviewQuery(Guid PurchaseInvoiceId) : IRequest<ThreeWayMatchPreviewDto>;

public class GetThreeWayMatchPreviewQueryHandler : IRequestHandler<GetThreeWayMatchPreviewQuery, ThreeWayMatchPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPurchaseMatchingService _matchingService;

    public GetThreeWayMatchPreviewQueryHandler(IUnitOfWork unitOfWork, IPurchaseMatchingService matchingService)
    {
        _unitOfWork = unitOfWork;
        _matchingService = matchingService;
    }

    public async Task<ThreeWayMatchPreviewDto> Handle(GetThreeWayMatchPreviewQuery request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<PurchaseInvoice>().Query()
            .Include(i => i.Lines)
            .Include(i => i.Supplier)
            .FirstOrDefaultAsync(i => i.Id == request.PurchaseInvoiceId, cancellationToken);

        if (invoice == null)
            throw new PurchasingDomainException($"Purchase invoice {request.PurchaseInvoiceId} not found.");

        PurchaseOrder? purchaseOrder = null;
        if (invoice.PurchaseOrderId.HasValue)
        {
            purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().Query()
                .Include(po => po.Lines)
                .FirstOrDefaultAsync(po => po.Id == invoice.PurchaseOrderId.Value, cancellationToken);
        }

        var grnNumbers = new HashSet<string>();
        var previewLines = new List<ThreeWayMatchLineDto>();

        foreach (var line in invoice.Lines)
        {
            var item = await _unitOfWork.Repository<Item>().GetByIdAsync(line.ItemId);
            var row = new ThreeWayMatchLineDto
            {
                InvoiceLineId = line.Id,
                ItemId = line.ItemId,
                ItemName = item?.Name ?? line.ItemId.ToString(),
                ItemCode = item?.Code ?? string.Empty,
                InvoiceQuantity = line.Quantity,
                InvoiceUnitCost = line.UnitCost,
                InvoiceLineTotal = line.LineTotal
            };

            if (line.PurchaseOrderLineId.HasValue)
            {
                var poLine = purchaseOrder?.Lines.FirstOrDefault(l => l.Id == line.PurchaseOrderLineId.Value)
                    ?? await _unitOfWork.Repository<PurchaseOrderLine>().GetByIdAsync(line.PurchaseOrderLineId.Value);

                if (poLine != null)
                {
                    row.PoQuantity = poLine.Quantity;
                    row.PoUnitCost = poLine.UnitCost;
                    row.PoLineReference = purchaseOrder?.OrderNumber ?? poLine.PurchaseOrderId.ToString();

                    if (Math.Abs(line.UnitCost - poLine.UnitCost) > 0.001m)
                    {
                        row.HasPriceVariance = true;
                        row.Issues.Add($"Price: invoice {line.UnitCost} vs PO {poLine.UnitCost}");
                    }
                }
            }
            else if (invoice.PurchaseOrderId.HasValue)
            {
                row.MissingPoLink = true;
                row.Issues.Add("Missing PO line link");
            }

            if (line.GoodsReceiptLineId.HasValue)
            {
                var grnLine = await _unitOfWork.Repository<GoodsReceiptNoteLine>().Query()
                    .Include(gl => gl.GoodsReceiptNote)
                    .FirstOrDefaultAsync(gl => gl.Id == line.GoodsReceiptLineId.Value, cancellationToken);

                if (grnLine != null)
                {
                    row.GrnReceivedQuantity = grnLine.ReceivedQuantity;
                    row.GrnNumber = grnLine.GoodsReceiptNote?.GRNNumber;
                    if (!string.IsNullOrEmpty(row.GrnNumber))
                        grnNumbers.Add(row.GrnNumber);

                    if (line.Quantity > grnLine.ReceivedQuantity)
                    {
                        row.HasQuantityVariance = true;
                        row.Issues.Add($"Qty: invoice {line.Quantity} vs GRN {grnLine.ReceivedQuantity}");
                    }
                }
                else
                {
                    row.MissingGrnLink = true;
                    row.Issues.Add("GRN line not found");
                }
            }
            else if (invoice.PurchaseOrderId.HasValue)
            {
                row.MissingGrnLink = true;
                row.Issues.Add("Missing GRN line link");
            }

            previewLines.Add(row);
        }

        var (_, matchMessage, _) = await _matchingService.MatchInvoiceAsync(invoice);
        var hasIssues = previewLines.Any(l => l.Issues.Count > 0);

        return new ThreeWayMatchPreviewDto
        {
            PurchaseInvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            SupplierInvoiceNumber = invoice.SupplierInvoiceNumber,
            Status = invoice.Status.ToString(),
            PurchaseOrderId = invoice.PurchaseOrderId,
            PurchaseOrderNumber = purchaseOrder?.OrderNumber,
            GoodsReceiptNumbers = grnNumbers.ToList(),
            Lines = previewLines,
            CanMatch = !hasIssues && invoice.Lines.Count > 0,
            SummaryMessage = hasIssues ? "Variances detected — review before matching." : matchMessage
        };
    }
}
