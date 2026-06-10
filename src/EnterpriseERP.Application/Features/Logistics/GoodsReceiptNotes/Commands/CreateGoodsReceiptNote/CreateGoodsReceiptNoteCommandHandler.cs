using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Features.Logistics.GoodsReceiptNotes.Commands.CreateGoodsReceiptNote;

public class CreateGoodsReceiptNoteCommandHandler : IRequestHandler<CreateGoodsReceiptNoteCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateGoodsReceiptNoteCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateGoodsReceiptNoteCommand request, CancellationToken cancellationToken)
    {
        try 
        {
            var grn = new GoodsReceiptNote
            {
                Id = Guid.NewGuid(),
                GRNNumber = $"GRN-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ReceiptDate = request.ReceiptDate,
                PurchaseOrderId = request.PurchaseOrderId,
                SupplierId = request.SupplierId,
                WarehouseId = request.WarehouseId,
                ReferenceNumber = request.ReferenceNumber,
                Notes = request.Notes,
                Status = GoodsReceiptStatus.Draft
            };

            foreach (var line in request.Lines)
            {
                grn.Lines.Add(new GoodsReceiptLine
                {
                    Id = Guid.NewGuid(),
                    GoodsReceiptNoteId = grn.Id,
                    ItemId = line.ItemId,
                    OrderedQuantity = line.OrderedQuantity,
                    ReceivedQuantity = line.ReceivedQuantity
                });
            }

            await _unitOfWork.Repository<GoodsReceiptNote>().AddAsync(grn);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(grn.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(new Error("GRN.CreateError", ex.Message));
        }
    }
}
