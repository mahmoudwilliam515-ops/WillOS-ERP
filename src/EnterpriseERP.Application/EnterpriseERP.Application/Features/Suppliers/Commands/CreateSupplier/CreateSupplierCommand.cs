using EnterpriseERP.Application.Features.Suppliers.DTOs;
using MediatR;

namespace EnterpriseERP.Application.Features.Suppliers.Commands.CreateSupplier;

public class CreateSupplierCommand : IRequest<SupplierDto>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public decimal CreditLimit { get; set; }
    public decimal OpeningBalance { get; set; }
    public string Notes { get; set; } = string.Empty;
}
