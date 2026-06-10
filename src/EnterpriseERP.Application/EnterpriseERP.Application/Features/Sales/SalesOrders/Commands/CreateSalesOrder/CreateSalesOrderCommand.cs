using MediatR;
using EnterpriseERP.SharedKernel.Results;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Commands.CreateSalesOrder;

public record CreateSalesOrderCommand(
    Guid CustomerId,
    Guid CompanyId,
    DateTime OrderDate,
    List<CreateSalesOrderLineDto> Lines,
    Guid TenantId,
    DateTime? RequestedDeliveryDate = null,
    string? CustomerReference = null,
    string? DeliveryAddress = null,
    string? Notes = null,
    Guid? SalesRepId = null
) : IRequest<Result<Guid>>;

public record CreateSalesOrderLineDto(
    Guid ItemId,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent = 0,
    decimal TaxPercent = 0,
    Guid? WarehouseId = null
);
