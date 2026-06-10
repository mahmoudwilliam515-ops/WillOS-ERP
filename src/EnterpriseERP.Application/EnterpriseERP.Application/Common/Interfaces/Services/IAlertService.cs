using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface IAlertService
{
    /// <summary>
    /// Scans the system for manufacturing issues and triggers alerts.
    /// </summary>
    Task CheckManufacturingAlertsAsync(CancellationToken cancellationToken = default);
}
