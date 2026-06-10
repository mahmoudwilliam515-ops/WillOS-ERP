namespace EnterpriseERP.Application.Common.Interfaces.Services;

public interface ILegacyDataMigrationService
{
    /// <summary>
    /// Migrates legacy data (Items, Customers, Suppliers) from the provided legacy SQL Server database 
    /// to the new Enterprise ERP database, ensuring strict adherence to new constraints.
    /// </summary>
    /// <param name="legacyConnectionString">The connection string of the legacy SQL Server database.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A summary of the migration results.</returns>
    Task<string> MigrateLegacyDataAsync(string legacyConnectionString, CancellationToken cancellationToken = default);
}
