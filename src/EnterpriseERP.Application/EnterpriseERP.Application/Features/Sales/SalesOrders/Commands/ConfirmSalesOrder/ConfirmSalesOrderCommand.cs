using MediatR;
using System;
using EnterpriseERP.SharedKernel.Results;

namespace EnterpriseERP.Application.Features.Sales.SalesOrders.Commands.ConfirmSalesOrder;

public record ConfirmSalesOrderCommand : IRequest<Result>
{
    public Guid SalesOrderId { get; init; }
    public Guid CompanyId { get; init; }
}
