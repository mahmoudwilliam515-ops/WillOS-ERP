using System.Collections.Generic;
using System.Threading.Tasks;
using EnterpriseERP.Domain.Entities.Treasury;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IIso20022Service
{
    /// <summary>
    /// Generates a pain.001.001.03 XML file content for a list of payment vouchers.
    /// </summary>
    Task<string> GeneratePain001XmlAsync(IEnumerable<PaymentVoucher> vouchers);
}
