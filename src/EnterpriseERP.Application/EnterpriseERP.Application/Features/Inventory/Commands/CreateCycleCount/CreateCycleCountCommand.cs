using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.SharedKernel.Results;
using MediatR;

namespace EnterpriseERP.Application.Features.Inventory.Commands.CreateCycleCount;

public class CreateCycleCountCommand : IRequest<Result<Guid>>
{
    public Guid WarehouseId { get; set; }
    public DateTime CountDate { get; set; }
    public string? Notes { get; set; }
    public List<CycleCountLineRequest> Lines { get; set; } = new();
}

public class CycleCountLineRequest
{
    public Guid ItemId { get; set; }
    public decimal CountedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateCycleCountCommandHandler : IRequestHandler<CreateCycleCountCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCycleCountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateCycleCountCommand request, CancellationToken cancellationToken)
    {
        if (!request.Lines.Any())
            return Result.Failure<Guid>(new Error("CycleCount.NoLines", "Cycle count must include at least one item line."));

        var countNumber = $"CC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

        // For each line, compute system quantity from InventoryTransactions
        var lines = new List<CycleCountLine>();
        foreach (var lineReq in request.Lines)
        {
            var transactions = await _unitOfWork.Repository<InventoryTransaction>()
                .FindAsync(t => t.ItemId == lineReq.ItemId && t.WarehouseId == request.WarehouseId);
            var systemQty = transactions.Sum(t => t.Quantity);

            lines.Add(new CycleCountLine
            {
                Id = Guid.NewGuid(),
                ItemId = lineReq.ItemId,
                SystemQuantity = systemQty,
                CountedQuantity = lineReq.CountedQuantity,
                Notes = lineReq.Notes
            });
        }

        var cycleCount = new CycleCount
        {
            Id = Guid.NewGuid(),
            WarehouseId = request.WarehouseId,
            CountNumber = countNumber,
            CountDate = request.CountDate,
            Status = CycleCountStatus.Completed,
            Notes = request.Notes,
            Lines = lines
        };

        await _unitOfWork.Repository<CycleCount>().AddAsync(cycleCount);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(cycleCount.Id);
    }
}
