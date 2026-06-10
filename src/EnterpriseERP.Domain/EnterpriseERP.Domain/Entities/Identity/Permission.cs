using EnterpriseERP.SharedKernel.Common;

namespace EnterpriseERP.Domain.Entities.Identity;

public class Permission : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g. "CanCreateInvoice"
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
