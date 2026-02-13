using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ERPBackend.Core.Interfaces;
using ERPBackend.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ERPBackend.Services.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DatabaseService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<string> BackupDatabaseAsync()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            string databaseName = builder.InitialCatalog;
            string backupFileName = $"{databaseName}_{DateTime.Now:yyyyMMddHHmmss}.bak";

            // 1. Get SQL Server's default backup directory to avoid permission issues
            string queryBackupDir = @"
                DECLARE @BackupDir nvarchar(4000);
                EXEC master.dbo.xp_instance_regread
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer',
                    N'BackupDirectory',
                    @BackupDir OUTPUT;
                SELECT ISNULL(@BackupDir, 'C:\Temp');";

            string? sqlBackupDir = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(queryBackupDir, connection))
                {
                    sqlBackupDir = (string?)await command.ExecuteScalarAsync();
                }
            }

            if (string.IsNullOrEmpty(sqlBackupDir)) sqlBackupDir = @"C:\Temp";
            if (!Directory.Exists(sqlBackupDir)) Directory.CreateDirectory(sqlBackupDir);

            string sqlBackupPath = Path.Combine(sqlBackupDir, backupFileName);

            // 2. Perform the backup in the SQL-accessible directory
            string sql = $"BACKUP DATABASE [{databaseName}] TO DISK = '{sqlBackupPath}' WITH FORMAT, NAME = 'Full Backup of {databaseName}';";
            await _context.Database.ExecuteSqlRawAsync(sql);

            // 3. Copy the backup to our web directory for downloading
            string webBackupDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Backups");
            if (!Directory.Exists(webBackupDir)) Directory.CreateDirectory(webBackupDir);
            
            string webBackupPath = Path.Combine(webBackupDir, backupFileName);
            
            // Wait a moment for SQL to release the file handle if necessary
            await Task.Delay(1000);
            File.Copy(sqlBackupPath, webBackupPath, true);

            return backupFileName;
        }

        public async Task<bool> RestoreDatabaseAsync(string backupFileName)
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            string databaseName = builder.InitialCatalog;
            string webBackupPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Backups", backupFileName);

            if (!File.Exists(webBackupPath))
                throw new FileNotFoundException("Backup file not found in web directory.");

            // 1. Get SQL Server's default backup directory
            string queryBackupDir = @"
                DECLARE @BackupDir nvarchar(4000);
                EXEC master.dbo.xp_instance_regread
                    N'HKEY_LOCAL_MACHINE',
                    N'Software\Microsoft\MSSQLServer\MSSQLServer',
                    N'BackupDirectory',
                    @BackupDir OUTPUT;
                SELECT ISNULL(@BackupDir, 'C:\Temp');";

            string? sqlBackupDir = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(queryBackupDir, connection))
                {
                    sqlBackupDir = (string?)await command.ExecuteScalarAsync();
                }
            }

            if (string.IsNullOrEmpty(sqlBackupDir)) sqlBackupDir = @"C:\Temp";
            string sqlRestorePath = Path.Combine(sqlBackupDir, backupFileName);

            // 2. Copy the file back to a directory SQL Server can read from
            if (!File.Exists(sqlRestorePath))
            {
                File.Copy(webBackupPath, sqlRestorePath, true);
            }

            // 3. To restore, we need to be in master and kill other connections
            string sql = $@"
                USE master;
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                RESTORE DATABASE [{databaseName}] FROM DISK = '{sqlRestorePath}' WITH REPLACE;
                ALTER DATABASE [{databaseName}] SET MULTI_USER;";

            using (var connection = new SqlConnection(_connectionString.Replace(databaseName, "master")))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(sql, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        public async Task<string> UploadBackupFileAsync(byte[] fileContent, string fileName)
        {
            string webBackupDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Backups");
            if (!Directory.Exists(webBackupDir)) Directory.CreateDirectory(webBackupDir);

            // Clean filename to prevent path injection
            string safeFileName = Path.GetFileName(fileName);
            if (!safeFileName.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Only .bak files are allowed.");

            string filePath = Path.Combine(webBackupDir, safeFileName);
            await File.WriteAllBytesAsync(filePath, fileContent);

            return safeFileName;
        }
    }
}
