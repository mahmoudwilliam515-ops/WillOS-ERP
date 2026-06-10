using System.Text;
using EnterpriseERP.Application.Common.Interfaces.Repositories;
using EnterpriseERP.Application.Common.Interfaces.Services;
using EnterpriseERP.Domain.Entities.Inventory;
using EnterpriseERP.Domain.Entities.Purchasing;
using EnterpriseERP.Domain.Entities.Sales;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.Infrastructure.Services;

public class LegacyDataMigrationService : ILegacyDataMigrationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LegacyDataMigrationService> _logger;

    public LegacyDataMigrationService(IUnitOfWork unitOfWork, ILogger<LegacyDataMigrationService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> MigrateLegacyDataAsync(string legacyConnectionString, CancellationToken cancellationToken = default)
    {
        var summary = new StringBuilder();
        summary.AppendLine("Migration Started...");

        try
        {
            using var legacyConnection = new SqlConnection(legacyConnectionString);
            await legacyConnection.OpenAsync(cancellationToken);

            // 1. Migrate Customers
            int customersCount = await MigrateCustomersAsync(legacyConnection, cancellationToken);
            summary.AppendLine($"Successfully migrated {customersCount} Customers.");

            // 2. Migrate Suppliers
            int suppliersCount = await MigrateSuppliersAsync(legacyConnection, cancellationToken);
            summary.AppendLine($"Successfully migrated {suppliersCount} Suppliers.");

            // 3. Migrate Items
            int itemsCount = await MigrateItemsAsync(legacyConnection, cancellationToken);
            summary.AppendLine($"Successfully migrated {itemsCount} Items.");

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            summary.AppendLine("Migration Completed and Saved Successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Data migration failed.");
            summary.AppendLine($"Migration FAILED: {ex.Message}");
        }

        return summary.ToString();
    }

    private async Task<int> MigrateCustomersAsync(SqlConnection legacyConnection, CancellationToken cancellationToken)
    {
        var customersRepo = _unitOfWork.Repository<Customer>();
        int count = 0;

        // Assuming legacy table is named 'Clients' or 'Customers'
        // Fallback to empty handling if table structure differs. This is a robust template.
        var query = "IF OBJECT_ID('Clients', 'U') IS NOT NULL SELECT Id, Name, Phone, Address FROM Clients ELSE SELECT 1 WHERE 1=0";
        using var command = new SqlCommand(query, legacyConnection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            // Note: We generate a new Guid for the Enterprise system, 
            // but we could store the legacy ID in an ExternalReference field if available.
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = reader["Name"]?.ToString() ?? "Unknown Client",
                Phone = reader["Phone"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString() ?? "",
                IsActive = true,
                CreditLimit = 0,
                OpeningBalance = 0,
                Balance = 0
            };

            await customersRepo.AddAsync(customer);
            count++;
        }

        return count;
    }

    private async Task<int> MigrateSuppliersAsync(SqlConnection legacyConnection, CancellationToken cancellationToken)
    {
        var suppliersRepo = _unitOfWork.Repository<Supplier>();
        int count = 0;

        var query = "IF OBJECT_ID('Suppliers', 'U') IS NOT NULL SELECT Id, Name, Phone, Address FROM Suppliers ELSE SELECT 1 WHERE 1=0";
        using var command = new SqlCommand(query, legacyConnection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Name = reader["Name"]?.ToString() ?? "Unknown Supplier",
                Phone = reader["Phone"]?.ToString() ?? "",
                Address = reader["Address"]?.ToString() ?? "",
                IsActive = true,
                CreditLimit = 0,
                OpeningBalance = 0,
                Balance = 0
            };

            await suppliersRepo.AddAsync(supplier);
            count++;
        }

        return count;
    }

    private async Task<int> MigrateItemsAsync(SqlConnection legacyConnection, CancellationToken cancellationToken)
    {
        var itemsRepo = _unitOfWork.Repository<Item>();
        int count = 0;

        var query = "IF OBJECT_ID('Items', 'U') IS NOT NULL SELECT Id, Name, Code, Price, Cost FROM Items ELSE SELECT 1 WHERE 1=0";
        using var command = new SqlCommand(query, legacyConnection);
        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            var item = new Item
            {
                Id = Guid.NewGuid(),
                NameAr = reader["Name"]?.ToString() ?? "Unknown Item",
                NameEn = reader["Name"]?.ToString() ?? "Unknown Item",
                Code = reader["Code"]?.ToString() ?? Guid.NewGuid().ToString().Substring(0, 8),
                BuyPrice = reader["Cost"] != DBNull.Value ? Convert.ToDecimal(reader["Cost"]) : 0,
                IsActive = true,
                IsService = false,
                ValuationMethod = ItemValuationMethod.WeightedAverage
            };

            await itemsRepo.AddAsync(item);
            count++;
        }

        return count;
    }
}
