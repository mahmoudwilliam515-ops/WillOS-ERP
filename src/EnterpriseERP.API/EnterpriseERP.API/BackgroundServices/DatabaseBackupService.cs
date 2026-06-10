using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EnterpriseERP.API.BackgroundServices;

/// <summary>
/// Background service that performs an automated daily backup of the SQL Server database.
/// Configured to run at a specific time (e.g., 2:00 AM) and keeps backups for a specific retention period.
/// </summary>
public class DatabaseBackupService : BackgroundService
{
    private readonly ILogger<DatabaseBackupService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _backupFolder;
    private readonly int _retentionDays;

    public DatabaseBackupService(ILogger<DatabaseBackupService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Ensure backup folder exists
        _backupFolder = _configuration["BackupSettings:FolderPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");
        _retentionDays = int.TryParse(_configuration["BackupSettings:RetentionDays"], out var days) ? days : 7;

        if (!Directory.Exists(_backupFolder))
        {
            Directory.CreateDirectory(_backupFolder);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DatabaseBackupService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            
            // Assuming we want to backup at 2 AM UTC every day
            var nextBackupTime = now.Date.AddHours(2);
            if (now > nextBackupTime)
            {
                nextBackupTime = nextBackupTime.AddDays(1);
            }

            var delay = nextBackupTime - now;
            _logger.LogInformation("Next database backup scheduled in {DelayHours} hours.", delay.TotalHours);

            // Wait until the scheduled time
            await Task.Delay(delay, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformBackupAsync(stoppingToken);
                    CleanUpOldBackups();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while performing the database backup.");
                }
            }
        }
    }

    private async Task PerformBackupAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Backup skipped: No DefaultConnection found in configuration.");
            return;
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        
        if (string.IsNullOrEmpty(databaseName))
        {
            _logger.LogWarning("Backup skipped: Database name could not be determined from connection string.");
            return;
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{databaseName}_Backup_{timestamp}.bak";
        var backupFilePath = Path.Combine(_backupFolder, backupFileName);

        _logger.LogInformation("Starting database backup for {DatabaseName} to {FilePath}", databaseName, backupFilePath);

        var query = $"BACKUP DATABASE [{databaseName}] TO DISK = @BackupFilePath WITH FORMAT, MEDIANAME = 'DB_Backup', NAME = 'Full Backup of {databaseName}';";

        using var connection = new SqlConnection(connectionString);
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@BackupFilePath", backupFilePath);

        await connection.OpenAsync(cancellationToken);
        await command.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Database backup completed successfully.");
    }

    private void CleanUpOldBackups()
    {
        _logger.LogInformation("Starting cleanup of backups older than {RetentionDays} days.", _retentionDays);

        var directoryInfo = new DirectoryInfo(_backupFolder);
        var files = directoryInfo.GetFiles("*.bak");

        foreach (var file in files)
        {
            if (file.CreationTimeUtc < DateTime.UtcNow.AddDays(-_retentionDays))
            {
                try
                {
                    file.Delete();
                    _logger.LogInformation("Deleted old backup file: {FileName}", file.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete old backup file: {FileName}", file.Name);
                }
            }
        }
    }
}
