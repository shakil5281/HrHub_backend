using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPBackend.Core.Interfaces
{
    public interface IDatabaseService
    {
        Task<string> BackupDatabaseAsync();
        Task<bool> RestoreDatabaseAsync(string backupFilePath);
        Task<string> UploadBackupFileAsync(byte[] fileContent, string fileName);
    }
}
