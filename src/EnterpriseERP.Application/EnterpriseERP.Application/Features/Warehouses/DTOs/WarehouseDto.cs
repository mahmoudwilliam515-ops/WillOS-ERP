namespace EnterpriseERP.Application.Features.Warehouses.DTOs;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public bool IsActive { get; set; }
}
